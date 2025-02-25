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

    public async Task<Result<bool>> UploadFile(string localPath, string remotePath)
    {
        var resulta = _backgroundService.QueueFileUpload(localPath, remotePath, "192.168.8.9", 21);
        return Result.Success(true);
        /*
        try
        {
            var client = new FtpClient("192.168.8.124", 5000);

            client.Connect();

            client.UploadFile(localPath, "/artofglide2.nsz");
            client.Disconnect();
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(ex.Message);
        }
        */
    }
}