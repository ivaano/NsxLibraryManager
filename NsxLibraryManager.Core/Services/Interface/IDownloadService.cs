using NsxLibraryManager.Shared.Settings;

namespace NsxLibraryManager.Core.Services.Interface;

public interface IDownloadService
{
    Task<string> GetVersionsFile(string versionUrl, CancellationToken cancellationToken);
    
    Task<string> GetLatestTitleDb(DownloadSettings downloadSettings, CancellationToken cancellationToken);
    Task DownloadFileAsync(string url, string destFilePath, int timeOut, CancellationToken cancellationToken);
}