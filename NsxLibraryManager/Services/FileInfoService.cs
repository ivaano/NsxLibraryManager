using LibHac.Ncm;
using NsxLibraryManager.Exceptions;
using NsxLibraryManager.FileLoading.QuickFileInfoLoading;
using NsxLibraryManager.Models;

namespace NsxLibraryManager.Services;

public class FileInfoService : IFileInfoService
{
    private readonly IPackageInfoLoader _packageInfoLoader;
    private readonly IDataService _dataService;
    private readonly ILogger<FileInfoService> _logger;
    private IEnumerable<string> _directoryFiles = new List<string>();

    
    public FileInfoService(
            IPackageInfoLoader packageInfoLoader, 
            IDataService dataService,
            ILogger<FileInfoService> logger)
    {
        _packageInfoLoader = packageInfoLoader ?? throw new ArgumentNullException(nameof(packageInfoLoader));
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
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
            Type = packageInfo.Contents.First().Type,
            PackageType = packageInfo.AccuratePackageType,
            FileName = filePath
        };
        
        if (title.Type != ContentMetaType.Application)
        {
            title.ApplicationTitleName = _dataService.RegionRepository("US").GetTitleById(packageInfo.Contents.First().ApplicationTitleId)?.Name;
        }

        if (packageInfo.Contents.First().NacpData is null)
        {
            var titleDb = _dataService.RegionRepository("US").FindTitleByIds(packageInfo.Contents.First().TitleId);

            if (titleDb is null)
            {
                title.Error = true;
                title.ErrorMessage = "Title not found in the nca or the database (Is it on a different region?).";
                return await Task.FromResult(title);
            } 
            
            title.TitleName = titleDb.Name ?? string.Empty;
            title.Publisher = titleDb.Publisher ?? string.Empty;
            return await Task.FromResult(title);
        }

        var nacpData = packageInfo.Contents.First().NacpData;
        if (nacpData != null)
            foreach (var ncpTitle in nacpData.Titles)
            {
                if (ncpTitle is null) continue;
                title.TitleName = ncpTitle.Name;
                title.Publisher = ncpTitle.Publisher;
                break;
            }

        return await Task.FromResult(title);
    }
}