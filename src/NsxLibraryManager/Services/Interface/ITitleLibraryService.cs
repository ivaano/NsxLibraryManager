using Common.Services;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Models;
using NsxLibraryManager.Models.NsxLibrary;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using Radzen;

namespace NsxLibraryManager.Services.Interface;

public interface ITitleLibraryService
{
    public Task<bool> DropLibrary();
    public Task<IEnumerable<string>> GetLibraryFilesAsync();
    public Task<FileDelta> GetDeltaFilesInLibraryAsync();
    public Task<Title?> ProcessFileAsync(string file);
    public Task<Result<bool>> AddLibraryTitleAsync(LibraryTitleDto title);
    public Task<Result<int>> UpdateMultipleLibraryTitlesAsync(IEnumerable<LibraryTitleDto> titles);
    public Task<Result<int>> UpdateLibraryTitleAsync(LibraryTitleDto titleDto);
    public Task<Result<bool>> RemoveLibraryTitleAsync(LibraryTitleDto title);
    public Task<bool> SaveContentCounts(Dictionary<string, int> updateCounts, TitleContentType contentType);
    public Task<Result<LibraryTitleDto>> GetTitleByApplicationId(string applicationId);
    public int GetTitlesCountByContentType(TitleContentType contentType);
    public Task<Result<GetBaseTitlesResultDto>> GetDuplicateTitles(TitleContentType contentType);
    public Task<Result<bool>> RemoveDuplicateTitles(IList<LibraryTitleDto> titleDtos);
    public Task<Result<bool>> RemoveDuplicateTitle(LibraryTitleDto titleDto);
    public Task<IQueryable<DlcDto>> GetTitleDlcsAsQueryable(string applicationId);
    public Task<Result<GetBaseTitlesResultDto>> GetTitles(LoadDataArgs args);
    public Task<GetBaseTitlesResultDto> GetBaseTitlesWithMissingLastUpdate(LoadDataArgs args);
    public Task<GetBaseTitlesResultDto> GetDlcTitlesWithMissingLastUpdate(LoadDataArgs args);
    public Task<GetBaseTitlesResultDto> GetBaseTitlesWithMissingDlc(LoadDataArgs args);
    public Task SaveLibraryReloadDate(bool refresh = false);
    public Task<LibraryUpdate?> GetLastLibraryUpdateAsync();
    public Task<Result<IEnumerable<string>>> GetCategoriesAsync();
    public Task<Result<IEnumerable<CollectionDto>>> GetCollections();
    public Task<Result<CollectionDto?>> AddCollection(CollectionDto collectionDto);
    public Task<Result<CollectionDto?>> RemoveCollection(CollectionDto collectionDto);
    public Task<Result<CollectionDto?>> UpdateCollection(CollectionDto collectionDto);
    public Task<Result<IEnumerable<RenameTitleDto>>> GetLibraryFilesToRenameAsync(RenameType renameType);

}