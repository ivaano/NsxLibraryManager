using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Models.NsxLibrary;

namespace NsxLibraryManager.Services.Interface;

public interface ISqlTitleLibraryService
{
    public Task<bool> DropLibrary();
    public Task<IEnumerable<string>> GetFilesAsync();
    
    public Task<Title?> ProcessFileAsync(string file);
    public Task<bool> SaveDatabaseChangesAsync();

    public Task<bool> SaveContentCounts(Dictionary<string, int> updateCounts, TitleContentType contentType);

    public Task<LibraryTitleDto?> GetTitleByApplicationId(string applicationId);
    
    public Task<IQueryable<DlcDto>> GetTitleDlcsAsQueryable(string applicationId);
}