using Common.Services;
using FluentFTP;
using NsxLibraryManager.Services.Interface;
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

    public async Task<Result<bool>> UploadFile(string localPath, string remotePath, string host, int port)
    {
        var resulta = _backgroundService.QueueFileUpload(localPath, remotePath, host, port);
        return Result.Success(true);
    }
}