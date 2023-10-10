namespace NsxLibraryManager.Services;

public interface IDownloadService
{
    Task<string> GetRegionFile(string region, CancellationToken cancellationToken);
    Task<string> GetCnmtsFile(CancellationToken cancellationToken);
    Task<string> GetVersionsFile(CancellationToken cancellationToken);
    Task DownloadFileAsync(string url, string destFilePath, CancellationToken cancellationToken);
}