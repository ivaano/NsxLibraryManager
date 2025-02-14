using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Data;
using NsxLibraryManager.Services.Interface;
using Radzen;

namespace NsxLibraryManager.Pages.Components;
#nullable disable

public partial class RefreshTitleDbProgressDialog : IDisposable
{
    private string DownloadingInfo { get; set; }

    [Inject] protected DialogService DialogService { get; set; }
    [Inject] protected ISettingsService SettingsService { get; set; }
    
    [Inject] protected IDownloadService DownloadService { get; set; }
    
    [Inject] protected ITitledbService TitledbService { get; set; }
    [Inject] protected TitledbDbContext DbContext { get; set; } = default!;
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await DoWork();
        }
    }
    
    private async Task DoWork(CancellationToken cancellationToken = default)
    {
        await InvokeAsync(
                async () =>
                {
                    DownloadingInfo = "Checking latest titledb version...";
                    StateHasChanged();

                    var settings = SettingsService.GetUserSettings();
                    var latestVersion = await DownloadService.GetVersionsFile(settings.DownloadSettings.VersionUrl, cancellationToken);
                    var dbHistory = DbContext.History.OrderByDescending(h => h.TimeStamp).FirstOrDefault();
                    var getNewVersion = false;
                    if (dbHistory is not null)
                    {
                        if (dbHistory.VersionNumber != latestVersion)
                        {
                            getNewVersion = true;
                        }
                    }
                    else
                    {
                        getNewVersion = true;
                    }

                    if (getNewVersion)
                    {
                        DownloadingInfo = $"Downloading version titledb version {latestVersion} ...";
                        StateHasChanged();

                        var compressedFilePath = await DownloadService.GetLatestTitleDb(settings.DownloadSettings, cancellationToken);
                        await TitledbService.ReplaceDatabase(compressedFilePath, cancellationToken);
                    }
                });
        DialogService.Close();
    }
    
    public void Dispose()
    {
        DialogService.Dispose();    }
}