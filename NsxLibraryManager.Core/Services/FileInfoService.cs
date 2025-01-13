using System.Text.RegularExpressions;
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
    private readonly ILogger<FileInfoService> _logger;
    private IEnumerable<string> _directoryFiles = new List<string>();
    private const string NszExtension = ".nsz";
    private const string NspExtension = ".nsp";
    private const string XciExtension = ".xci";
    private const string XczExtension = ".xcz";
    private const string TitleFilePattern = @"^(.*?)(?:\s*\[(DLC.*?)\])?\s*\[([0-9a-fA-F]+)\]\[v(\d+)\]";
    const string TitleIdPattern = @"\[([0-9A-F]{16})\]";
    const string TitleNamePattern = @"^(.*?)(?:\s*\[)";

    public FileInfoService(
            IPackageInfoLoader packageInfoLoader, 
            ILogger<FileInfoService> logger)
    {
        _packageInfoLoader = packageInfoLoader ?? throw new ArgumentNullException(nameof(packageInfoLoader));
        _logger = logger;
    }

    public Task<bool> IsDirectoryEmpty(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return Task.FromResult(false);
        try
        {
            return Task.FromResult(!Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Any());
        }
        catch (UnauthorizedAccessException)
        {
            return Task.FromResult(false); 
        }
        catch (DirectoryNotFoundException)
        {
            return Task.FromResult(false); 
        }
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
    public bool TryGetFileInfoFromFileName(string fileName, out LibraryTitle libraryTitle)
    {
        var fileNameNoExt = Path.GetFileNameWithoutExtension(fileName); 
        var extension = Path.GetExtension(fileName); 
        var dlcName = string.Empty;
        libraryTitle = new LibraryTitle
        {
            TitleId = string.Empty,
            FileName = string.Empty,
        };

        var match = Regex.Match(fileNameNoExt, TitleFilePattern);
        if (match.Success)
        {
            libraryTitle.TitleName = match.Groups[1].Value.Trim();
            dlcName = match.Groups[2].Value.Trim();
            libraryTitle.TitleId = match.Groups[3].Value;
            _ = int.TryParse(match.Groups[4].Value, out var versionNumber) ? versionNumber : 0;
            libraryTitle.TitleVersion = (uint)versionNumber;
            if (string.IsNullOrEmpty(dlcName) && versionNumber == 0)
            {
                libraryTitle.Type = TitleLibraryType.Base;
            } 
            
            if (string.IsNullOrEmpty(dlcName) && versionNumber > 0)
            {
                libraryTitle.Type = TitleLibraryType.Update;
            }
            
            if (!string.IsNullOrEmpty(dlcName))
            {
                if (dlcName.StartsWith("DLC", StringComparison.OrdinalIgnoreCase))
                {
                    dlcName = dlcName[3..].Trim(); 
                }
                libraryTitle.Type = TitleLibraryType.DLC;
                libraryTitle.TitleName = $"{libraryTitle.TitleName} - {dlcName}"; 
            }
        }
        
        if (string.IsNullOrEmpty(libraryTitle.TitleId))
        {
            //Try matching only title id [xxxx]
            var matchId = Regex.Match(fileNameNoExt, TitleIdPattern);
            if (matchId.Success)
            {
                libraryTitle.TitleId = matchId.Groups[1].Value.Trim();
            }
            else
            {
                return false;
            }
        }
        
        var packageType = extension switch
        {
            _ when extension.Equals(NszExtension, StringComparison.OrdinalIgnoreCase) => AccuratePackageType.NSZ,
            _ when extension.Equals(NspExtension, StringComparison.OrdinalIgnoreCase) => AccuratePackageType.NSP,
            _ when extension.Equals(XciExtension, StringComparison.OrdinalIgnoreCase) => AccuratePackageType.XCI,
            _ when extension.Equals(XczExtension, StringComparison.OrdinalIgnoreCase) => AccuratePackageType.XCZ,
            _ => AccuratePackageType.Unknown
        };
        
        if (packageType == AccuratePackageType.Unknown)
        {
            return false;
        }
        libraryTitle.PackageType = packageType;
        libraryTitle.Size = new FileInfo(fileName).Length;
        libraryTitle.FileName = Path.GetFullPath(fileName);
        libraryTitle.LastWriteTime = File.GetLastWriteTime(fileName);
        
        return true;            
    }
    
    
    public async Task<LibraryTitle> GetFileInfoFromFileName(string fileName)
    {
        
        //const string pattern = @"^(.*?)(?:\s*\[(DLC\s*(.*?))\])?\s*\[([0-9a-fA-F]+)\]\[v(\d+)\]";

        var fileNameNoExt = Path.GetFileNameWithoutExtension(fileName); 
        var directory = Path.GetDirectoryName(fileName); 
        var extension = Path.GetExtension(fileName); 
        var name = string.Empty;
        var dlcName = string.Empty;
        var id = string.Empty;
        var version = string.Empty;        
        var match = Regex.Match(fileNameNoExt, TitleFilePattern);

        if (match.Success)
        {
            name = match.Groups[1].Value.Trim();
            dlcName = match.Groups[2].Value.Trim();
            id = match.Groups[3].Value;
            version = match.Groups[4].Value;
        }
        
        if (string.IsNullOrEmpty(id))
        {
            var matchId = Regex.Match(fileNameNoExt, TitleIdPattern);
            if (matchId.Success)
            {
                id = matchId.Groups[1].Value.Trim();
            }
            else
            {
                
            }
        }
        var packageType = extension switch
        {
            _ when extension.Equals(NszExtension, StringComparison.OrdinalIgnoreCase) => AccuratePackageType.NSZ,
            _ when extension.Equals(NspExtension, StringComparison.OrdinalIgnoreCase) => AccuratePackageType.NSP,
            _ when extension.Equals(XciExtension, StringComparison.OrdinalIgnoreCase) => AccuratePackageType.XCI,
            _ when extension.Equals(XczExtension, StringComparison.OrdinalIgnoreCase) => AccuratePackageType.XCZ,
            _ => throw new ArgumentOutOfRangeException($"Unknown Extension for file {fileName}")
        };
        _ = int.TryParse(version, out var versionNumber) ? versionNumber : 0;


            
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
    
    public Task<LibraryTitle> GetFileInfo(string filePath, bool detailed)
    {
        PackageInfo packageInfo;
        try
        {
            packageInfo = _packageInfoLoader.GetPackageInfo(filePath, detailed);
        } 
        catch (Exception)
        {
            _logger.LogError("Error getting package info from file {filePath}", filePath);
            throw new Exception($"Error getting package info from file {filePath}");
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

        return Task.FromResult(title);
    }

}