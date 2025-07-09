using Common.Services;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Settings;

namespace NsxLibraryManager.Services;

public class FtpClientService : IFtpClientService
{
    private readonly UserSettings _userSettings;
    private readonly FtpBackgroundService _backgroundService;

    public FtpClientService(ISettingsService settingsService, IServiceProvider serviceProvider)
    {
        _userSettings = settingsService.GetUserSettings();
        _backgroundService = serviceProvider.GetServices<IHostedService>().OfType<FtpBackgroundService>().First();
    }

    public Task<Result<bool>> QueueFileUpload(string localPath, string remotePath, string host, int port)
    {
        _ = _backgroundService.QueueFileUpload(localPath, remotePath, host, port);
        return Task.FromResult(Result.Success(true));
    }

    public Task<Result<bool>> RemoveQueuedFileUpload(string queuedFileId)
    {
        _backgroundService.RemoveQueuedFileUpload(queuedFileId);
        return Task.FromResult(Result.Success(true));
    }

    public Task<Result<List<FtpUploadRequest>>> GetQueuedFiles()
    {
        var queuedFiles = _backgroundService.GetUploadQueue();
        return Task.FromResult(Result.Success(queuedFiles));
    }
}