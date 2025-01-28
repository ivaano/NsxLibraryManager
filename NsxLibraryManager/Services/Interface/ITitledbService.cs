using NsxLibraryManager.Common;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.ViewModels.TitleDb;
using Radzen;

namespace NsxLibraryManager.Services.Interface;

public interface ITitledbService
{
    public Task<LibraryTitleDto?> GetTitleByApplicationId(string applicationId);
    public Result<DbHistoryDto> GetLatestTitledbVersionAsync();

    public Task<Result<IEnumerable<string>>> GetCategoriesAsync();
    public Task<Result<GridPageViewModel>> GetTitles(LoadDataArgs args, IEnumerable<string> categories);
    Task ReplaceDatabase(string compressedFilePath, CancellationToken cancellationToken);
}