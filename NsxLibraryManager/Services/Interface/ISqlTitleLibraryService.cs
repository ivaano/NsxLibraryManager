using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Models;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Models.NsxLibrary;

namespace NsxLibraryManager.Services.Interface;

public interface ISqlTitleLibraryService
{
    public Task<bool> DropLibrary();
    public Task<IEnumerable<string>> GetFilesAsync();
    
    public Task<Title?> ProcessFileAsync(string file);
    public Task<bool> UpdateLibraryTitleAsync(LibraryTitle title);
    public Task<bool> RemoveLibraryTitleAsync(LibraryTitle title);

    public Task<bool> SaveContentCounts(Dictionary<string, int> updateCounts, TitleContentType contentType);
    public Task<FileDelta> GetDeltaFilesInLibraryAsync();

    public Task<LibraryTitleDto?> GetTitleByApplicationId(string applicationId);

    public Task<bool> SaveDatabaseChangesAsync();

    public Task<IQueryable<DlcDto>> GetTitleDlcsAsQueryable(string applicationId);
    
}