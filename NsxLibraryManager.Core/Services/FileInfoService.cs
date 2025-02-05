using System.Text.RegularExpressions;
using Common.Services;
using LibHac.Ncm;
using Microsoft.Extensions.Logging;
using NsxLibraryManager.Core.Extensions;
using NsxLibraryManager.Core.FileLoading;
using NsxLibraryManager.Core.FileLoading.Interface;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;

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
    
   
    public  async Task<Result<IEnumerable<string>>> GetFileNames(
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
            return Result.Failure<IEnumerable<string>>($"Invalid file path {filePath}");
        }
        return Result.Success<IEnumerable<string>>(fileList);
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
                libraryTitle.Type = TitleContentType.Base;
            } 
            
            if (string.IsNullOrEmpty(dlcName) && versionNumber > 0)
            {
                libraryTitle.Type = TitleContentType.Update;
            }
            
            if (!string.IsNullOrEmpty(dlcName))
            {
                if (dlcName.StartsWith("DLC", StringComparison.OrdinalIgnoreCase))
                {
                    dlcName = dlcName[3..].Trim(); 
                }
                libraryTitle.Type = TitleContentType.DLC;
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
            title.Type = TitleContentType.Base;
        } 
            
        if (string.IsNullOrEmpty(dlcName) && versionNumber > 0)
        {
            title.Type = TitleContentType.Update;
        }
            
        if (!string.IsNullOrEmpty(dlcName))
        {
            if (dlcName.StartsWith("DLC", StringComparison.OrdinalIgnoreCase))
            {
                dlcName = dlcName[3..].Trim(); 
            }
            title.Type = TitleContentType.DLC;
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
    
    public Task<Result<LibraryTitleDto>> GetFileInfo(string filePath, bool detailed)
    {
        PackageInfo packageInfo;
        try
        {
            packageInfo = _packageInfoLoader.GetPackageInfo(filePath, detailed);
            if (packageInfo.Contents is null)
                return Task.FromResult(Result.Failure<LibraryTitleDto>($"No contents found in the package of file {filePath}"));
        } 
        catch (Exception)
        {
            _logger.LogError("Error getting package info from file {filePath}", filePath);
            return Task.FromResult(Result.Failure<LibraryTitleDto>($"Error getting package info from file {filePath}"));
        }

        return Task.FromResult(Result.Success(packageInfo.ToLibraryTitleDto(filePath)));
    }

}