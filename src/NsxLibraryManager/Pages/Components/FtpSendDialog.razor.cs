using Common.Services;
using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using NsxLibraryManager.Shared.Settings;
using Radzen;

namespace NsxLibraryManager.Pages.Components;

public partial class FtpSendDialog : ComponentBase
{
    [Inject]
    protected NotificationService NotificationService { get; set; } = default!;
    
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; } = default!;

    [Inject] 
    protected IFtpClientService FtpClientService { get; set; } = default;
    
    [Inject]
    protected DialogService DialogService { get; set; } = default!;

    [Inject]
    protected ISettingsService SettingsService { get; set; } = default!;
    
    
    private FtpClientSettings _ftpClientSettings;
    private bool _disableButtons = false;
    
    
    [Parameter]
    public IList<LibraryTitleDto> SelectedTitles { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        _ftpClientSettings = await SettingsService.GetFtpClientSettings();
    }

    private async Task<Result<bool>> SendUpload(string? localPath)
    {
        if (localPath is null) return Result.Failure<bool>("Invalid file path");
        var fileName = Path.GetFileName(localPath);
        var remotePath = string.Join("/", [_ftpClientSettings.RemotePath, fileName]);
        var queueResult = await FtpClientService.QueueFileUpload(localPath, remotePath, _ftpClientSettings.Host, _ftpClientSettings.Port);
        return queueResult;
    }
    
    private async Task OnSubmit()
    {
        _disableButtons = true;
        await Task.Delay(1);
        await SettingsService.SaveFtpClientSettings(_ftpClientSettings);
        var queuedSuccessCount = 0;
        foreach (var title in SelectedTitles)
        {
            if (title.ContentType != TitleContentType.Base) continue;
            
            var titleResult = await TitleLibraryService.GetTitleByApplicationId(title.ApplicationId);
            if (titleResult.IsSuccess)
            {

                var queueResult = await SendUpload(titleResult.Value.FileName);
                if (queueResult.IsSuccess)
                {
                    queuedSuccessCount++;
                }

                var latestVersion = titleResult.Value.OwnedUpdates?.GetLatestVersion();
                if (latestVersion is not null)
                {
                    var result = await SendUpload(latestVersion.FileName);
                    if (result.IsSuccess)
                    {
                        queuedSuccessCount++;
                    }
                }

                if (titleResult.Value.OwnedDlcs is not null)
                {
                    foreach (var dlcDto in titleResult.Value.OwnedDlcs)
                    {
                        var result = await SendUpload(dlcDto.FileName);
                        if (result.IsSuccess)
                        {
                            queuedSuccessCount++;
                        }
                    }
                }
            }
        }
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info, 
            Summary = "File(s) successfully queued", 
            Detail = $"{queuedSuccessCount} files will be sent to the FTP server", 
            Duration = 4000
        });
        DialogService.Close(false);
    }
    
    private void Cancel()
    {
        DialogService.Close(false);
    }
}