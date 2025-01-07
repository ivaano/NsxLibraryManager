namespace NsxLibraryManager.Services.Interface;

public interface ITitledbService
{
    Task<string?> GetLatestTitledbVersionAsync();
    Task ReplaceDatabase(string compressedFilePath, CancellationToken cancellationToken);
}