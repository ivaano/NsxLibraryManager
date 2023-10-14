using LibHac.Ncm;
using NsxLibraryManager.Enums;
using NsxLibraryManager.Exceptions;
using NsxLibraryManager.FileLoading.QuickFileInfoLoading;
using NsxLibraryManager.Models;

namespace NsxLibraryManager.Services;

public class FileInfoService : IFileInfoService
{
    private readonly IPackageInfoLoader _packageInfoLoader;
    private readonly ITitleDbService _titleDbService;
    private readonly ILogger<FileInfoService> _logger;
    private IEnumerable<string> _directoryFiles = new List<string>();
   
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
    
    public IEnumerable<string> GetDirectoryFiles(string filePath)
    {
        var files = Directory.GetFiles(filePath);
        return files.ToList();
    }
    
    public async Task<IEnumerable<string>> GetRecursiveFiles(string filePath)
    {
        var files = Directory.GetFiles(filePath);
        _directoryFiles = _directoryFiles.Concat(files.ToList());
        
        var subdirectoryEntries = Directory.GetDirectories(filePath);
        foreach (var subdirectory in subdirectoryEntries)
        {
            await GetRecursiveFiles(subdirectory);
        }

        return _directoryFiles.ToList();
    }
    
    public async Task<LibraryTitle> GetFileInfo(string filePath)
    {
        var packageInfo = _packageInfoLoader.GetPackageInfo(filePath);
        
        if (packageInfo.Contents is null)
            throw new Exception("No contents found in the package");
        
        var title = new LibraryTitle
        {
            ApplicationTitleId = packageInfo.Contents.First().ApplicationTitleId,
            PatchTitleId = packageInfo.Contents.First().PatchTitleId,
            PatchNumber = packageInfo.Contents.First().PatchNumber,
            TitleId = packageInfo.Contents.First().TitleId,
            TitleVersion = packageInfo.Contents.First().Version.Version,
            PackageType = packageInfo.AccuratePackageType,
            FileName = Path.GetFullPath(filePath)
        };
        
        var availableVersion = await _titleDbService.GetAvailableVersion(title.TitleId);
        title.AvailableVersion = availableVersion >> 16;
        title.Type = packageInfo.Contents.First().Type switch
        {
                ContentMetaType.Application => TitleLibraryType.Base,
                ContentMetaType.Patch => TitleLibraryType.Update,
                ContentMetaType.AddOnContent => TitleLibraryType.DLC,
                _ => title.Type
        };

        var nacpData = packageInfo.Contents.First().NacpData;
        if (nacpData == null) return title;

        foreach (var ncpTitle in nacpData.Titles)
        {
            if (ncpTitle is null) continue;
            if (ncpTitle.Name == "") continue;
            title.TitleName = ncpTitle.Name;
            title.Publisher = ncpTitle.Publisher;
            break;
        }

        return title;
    }
}