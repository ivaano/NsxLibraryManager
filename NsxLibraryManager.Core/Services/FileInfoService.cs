﻿using System.Text.RegularExpressions;
using LibHac.Ncm;
using Microsoft.Extensions.Logging;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Exceptions;
using NsxLibraryManager.Core.Extensions;
using NsxLibraryManager.Core.FileLoading;
using NsxLibraryManager.Core.FileLoading.Interface;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services.Interface;

namespace NsxLibraryManager.Core.Services;

public class FileInfoService : IFileInfoService
{
    private readonly IPackageInfoLoader _packageInfoLoader;
    private readonly ITitleDbService _titleDbService;
    private readonly ILogger<FileInfoService> _logger;
    private IEnumerable<string> _directoryFiles = new List<string>();
    private const string NszExtension = ".nsz";
    private const string NspExtension = ".nsp";
    private const string XciExtension = ".xci";
    private const string XczExtension = ".xcz";
    private const string TitleFilePattern = @"^(.*?)(?:\s*\[(DLC.*?)\])?\s*\[([0-9a-fA-F]+)\]\[v(\d+)\]";
    public FileInfoService(
            IPackageInfoLoader packageInfoLoader, 
            ITitleDbService titleDbService,
            ILogger<FileInfoService> logger)
    {
        _packageInfoLoader = packageInfoLoader ?? throw new ArgumentNullException(nameof(packageInfoLoader));
        _titleDbService = titleDbService ?? throw new ArgumentNullException(nameof(titleDbService));
        _logger = logger;
    }
    
   
    public  async Task<IEnumerable<string>> GetFileNames(
            string filePath, 
            bool recursive = false)
    {
        var fileList = new List<string>();
        _directoryFiles = fileList;
        if (File.Exists(filePath))
        {
            fileList.Add(Path.GetFullPath(filePath));
        }
        else if (Directory.Exists(filePath) && !recursive)
        {
            var directoryFiles = GetDirectoryFiles(filePath);
            fileList.AddRange(directoryFiles);
        }
        else if (Directory.Exists(filePath) && recursive)
        {
            var recursiveFiles = await GetRecursiveFiles(filePath);
            fileList.AddRange(recursiveFiles);
        }
        else
        {
            throw new InvalidPathException(filePath);
        }
        return fileList;
    }

    public async Task<LibraryTitle> GetFileInfoFromFileName(string fileName)
    {
        
        //const string pattern = @"^(.*?)(?:\s*\[(DLC\s*(.*?))\])?\s*\[([0-9a-fA-F]+)\]\[v(\d+)\]";

        var fileNameNoExt = Path.GetFileNameWithoutExtension(fileName); 
        var directory = Path.GetDirectoryName(fileName); 
        string extension = Path.GetExtension(fileName); 
        
        var match = Regex.Match(fileNameNoExt, TitleFilePattern);

        if (!match.Success) throw new InvalidPathException(fileName);
        
        var name = match.Groups[1].Value.Trim();
        var dlcName = match.Groups[2].Value.Trim();
        var id = match.Groups[3].Value;
        var version = match.Groups[4].Value;
        var packageType = extension switch
        {
            _ when extension.Equals(NszExtension, StringComparison.OrdinalIgnoreCase) => AccuratePackageType.NSZ,
            _ when extension.Equals(NspExtension, StringComparison.OrdinalIgnoreCase) => AccuratePackageType.NSP,
            _ when extension.Equals(XciExtension, StringComparison.OrdinalIgnoreCase) => AccuratePackageType.XCI,
            _ when extension.Equals(XczExtension, StringComparison.OrdinalIgnoreCase) => AccuratePackageType.XCZ,
            _ => throw new ArgumentOutOfRangeException($"Unknown Extension for file {fileName}")
        };
        _ = int.TryParse(version, out var versionNumber) ? versionNumber : 0;

        if (string.IsNullOrEmpty(id))
        {
            throw new InvalidPathException(fileName);
        }
            
        var title = new LibraryTitle
        {
            TitleId = id,
            TitleName = name,
            TitleVersion = (uint)versionNumber,
            FileName = Path.GetFullPath(fileName),
            LastWriteTime = File.GetLastWriteTime(fileName),
            PackageType = packageType,
            Size = await GetFileSize(fileName),
        };  

        if (string.IsNullOrEmpty(dlcName) && versionNumber == 0)
        {
            title.Type = TitleLibraryType.Base;
        } 
            
        if (string.IsNullOrEmpty(dlcName) && versionNumber > 0)
        {
            title.Type = TitleLibraryType.Update;
        }
            
        if (!string.IsNullOrEmpty(dlcName))
        {
            if (dlcName.StartsWith("DLC", StringComparison.OrdinalIgnoreCase))
            {
                dlcName = dlcName[3..].Trim(); 
            }
            title.Type = TitleLibraryType.DLC;
            title.TitleName = $"{title.TitleName} - {dlcName}"; 
        }
        
        return title;
    }

    
    public IEnumerable<string> GetDirectoryFiles(string filePath)
    {
        var files = Directory.GetFiles(filePath);
        return files.ToList();
    }
    
    public Task<IEnumerable<string>> GetRecursiveFiles(string filePath)
    {
        var extensions = new[] { ".nsp", ".nsz", ".xci", ".xcz" };
        var files = Directory
            .EnumerateFiles(filePath, "*", SearchOption.AllDirectories)
            .Where(f => extensions.Contains(
                Path.GetExtension(f), 
                StringComparer.OrdinalIgnoreCase));
        
        return Task.FromResult<IEnumerable<string>>(files.ToList());
    }
    
    public Task<long?> GetFileSize(string filePath)
    {
        var size =  new FileInfo(filePath).Length;
        return Task.FromResult<long?>(size);
    }

    public async Task<string?> GetFileIcon(string filePath)
    {
        var packageInfo = _packageInfoLoader.GetPackageInfo(filePath, true);
        var iconUri = string.Empty;
        
        if (packageInfo.Contents is null)
            throw new Exception("No contents found in the package");

        if (packageInfo.Contents.Icon is not null)
        {
            var fileName = Guid.NewGuid() + ".jpg";
            var path = Path.Combine("images", "icon");
            var iconPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", path, fileName);
            await File.WriteAllBytesAsync(iconPath, packageInfo.Contents.Icon);
            iconUri = $"images/icon/{fileName}";
        }
        return iconUri;
    }
    
    public async Task<LibraryTitle?> GetFileInfo(string filePath, bool detailed)
    {
        PackageInfo packageInfo;
        try
        {
            packageInfo = _packageInfoLoader.GetPackageInfo(filePath, detailed);
        } 
        catch (Exception e)
        {
            _logger.LogError("Error getting package info from file {filePath}", filePath);
            return null;
        }

        if (packageInfo.Contents is null)
            throw new Exception($"No contents found in the package of file {filePath}");
        
        var title = new LibraryTitle
        {
            ApplicationTitleId = packageInfo.Contents.ApplicationTitleId,
            PatchTitleId = packageInfo.Contents.PatchTitleId,
            PatchNumber = packageInfo.Contents.PatchNumber,
            TitleId = packageInfo.Contents.TitleId,
            TitleName = packageInfo.Contents.Name,
            Publisher = packageInfo.Contents.Publisher,
            TitleVersion = packageInfo.Contents.Version.Version,
            PackageType = packageInfo.AccuratePackageType,
            FileName = Path.GetFullPath(filePath),
            LastWriteTime = File.GetLastWriteTime(filePath)
        };

        //var availableVersion = await _titleDbService.GetAvailableVersion(title.TitleId);
        //title.AvailableVersion = availableVersion >> 16;
        title.Type = packageInfo.Contents.Type switch
        {
                ContentMetaType.Application => TitleLibraryType.Base,
                ContentMetaType.Patch => TitleLibraryType.Update,
                ContentMetaType.AddOnContent => TitleLibraryType.DLC,
                _ => title.Type
        };

        return title;
    }

}