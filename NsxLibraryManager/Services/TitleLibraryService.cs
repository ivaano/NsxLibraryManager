using Microsoft.Extensions.Options;
using NsxLibraryManager.Models;
using NsxLibraryManager.Settings;

namespace NsxLibraryManager.Services;

public class TitleLibraryService : ITitleLibraryService
{
    private readonly IDataService _dataService;
    private readonly IFileInfoService _fileInfoService;
    private readonly AppSettings _configuration;

    
    public TitleLibraryService(IDataService dataService, IFileInfoService fileInfoService, IOptions<AppSettings> configuration)
    {
        _dataService = dataService;
        _fileInfoService = fileInfoService;
        _configuration = configuration.Value;
    }


    public async Task RefreshLibraryAsync()
    {
        var failedFiles = new List<string>();
        var files = await _fileInfoService.GetFileNames($"n:\\roms", true);
        foreach (var file in files)
        {
            try
            {
                var info = await _fileInfoService.GetFileInfo(file);
            }
            catch (Exception e)
            {
                failedFiles.Add(file);
            }
        }
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
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetFilesAsync()
    {
        var files = await _fileInfoService.GetFileNames($"n:\\roms validados", true);
        return files;
    }
}