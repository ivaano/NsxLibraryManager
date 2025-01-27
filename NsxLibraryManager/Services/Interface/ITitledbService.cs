using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Utils;

namespace NsxLibraryManager.Services.Interface;

public interface ITitledbService
{
    public Task<LibraryTitleDto?> GetTitleByApplicationId(string applicationId);
    public Result<DbHistoryDto> GetLatestTitledbVersionAsync();
    
    
    Task ReplaceDatabase(string compressedFilePath, CancellationToken cancellationToken);
}