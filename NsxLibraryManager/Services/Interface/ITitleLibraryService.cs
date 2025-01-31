using Common.Services;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Models;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Models.NsxLibrary;
using Radzen;

namespace NsxLibraryManager.Services.Interface;

public interface ITitleLibraryService
{
    public Task<bool> DropLibrary();
    public Task<IEnumerable<string>> GetFilesAsync();
    
    public Task<Title?> ProcessFileAsync(string file);
    public Task<bool> AddLibraryTitleAsync(LibraryTitle title);
    public Task<bool> UpdateLibraryTitleAsync(LibraryTitle title);
    public Task<bool> RemoveLibraryTitleAsync(LibraryTitle title);

    public Task<bool> SaveContentCounts(Dictionary<string, int> updateCounts, TitleContentType contentType);
    public Task<FileDelta> GetDeltaFilesInLibraryAsync();

    public Task<LibraryTitleDto?> GetTitleByApplicationId(string applicationId);
    public int GetTitlesCountByContentType(TitleContentType contentType);
    
    public Task<Result<GetBaseTitlesResultDto>> GetTitles(LoadDataArgs args);
    public Task<GetBaseTitlesResultDto> GetBaseTitlesWithMissingLastUpdate(LoadDataArgs args);
    public Task<GetBaseTitlesResultDto> GetBaseTitlesWithMissingDlc(LoadDataArgs args);

    public Task<bool> SaveDatabaseChangesAsync();

    public Task<IQueryable<DlcDto>> GetTitleDlcsAsQueryable(string applicationId);

    public Task SaveLibraryReloadDate(bool refresh = false);
    
    public Task<LibraryUpdate?> GetLastLibraryUpdateAsync();
    
    public Task<Result<IEnumerable<string>>> GetCategoriesAsync();

}