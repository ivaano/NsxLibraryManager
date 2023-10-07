using Microsoft.Extensions.Options;
using NsxLibraryManager.Settings;

namespace NsxLibraryManager.Services;

public class TitleLibraryService : ITitleLibraryService
{
    private readonly IDataService _dataService;
    private readonly IFileInfoService _fileInfoService;
    private readonly AppSettings _configuration;
    private readonly ILogger<TitleLibraryService> _logger;

    
    public TitleLibraryService(
            IDataService dataService, 
            IFileInfoService fileInfoService, 
            IOptions<AppSettings> configuration,
            ILogger<TitleLibraryService> logger)
    {
        _dataService = dataService;
        _fileInfoService = fileInfoService;
        _configuration = configuration.Value;
        _logger = logger;
    }
    
    public bool DropLibrary()
    {
        return _dataService.DropDbCollection(AppConstants.LibraryCollectionName);
    }

    public async Task<bool> ProcessFileAsync(string file)
    {
        try
        {
            var libraryTitle = await _fileInfoService.GetFileInfo(file);
            await _dataService.AddLibraryTitleAsync(libraryTitle);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error processing file: {file}");
            return false;
        }
    }

    public string GetLibraryPath()
    {
        return Path.GetFullPath(_configuration.LibraryPath);
    }

    public async Task<IEnumerable<string>> GetFilesAsync()
    {
        var files = await _fileInfoService.GetFileNames(_configuration.LibraryPath,
                                                                        _configuration.Recursive);
        return files;
    }
}