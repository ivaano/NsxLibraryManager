using NsxLibraryManager.Models.Dto;

namespace NsxLibraryManager.Services.Interface;

public interface ITitledbService
{
    public Task<LibraryTitleDto?> GetTitleByApplicationId(string applicationId);

    Task<string?> GetLatestTitledbVersionAsync();
    Task ReplaceDatabase(string compressedFilePath, CancellationToken cancellationToken);
}