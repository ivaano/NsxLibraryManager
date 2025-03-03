using Microsoft.AspNetCore.Components;
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

    [Inject] protected IFtpClientService FtpClientService { get; set; } = default;
    
    [Inject]
    protected DialogService DialogService { get; set; } = default!;

    private FtpClientSettings _ftpClientSettings;
    
    
    [Parameter]
    public IList<LibraryTitleDto> SelectedTitles { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        _ftpClientSettings = new FtpClientSettings
        {
            Host = "192.168.8.9",
            Port = 21,
            RemotePath = "/uploads"
        };
    }
    private async Task OnSubmit()
    {
        var queuedSuccessCount = 0;
        foreach (var title in SelectedTitles)
        {
            if (title.ContentType != TitleContentType.Base) continue;
            
            var titleResult = await TitleLibraryService.GetTitleByApplicationId(title.ApplicationId);
            if (titleResult.Value.FileName is not null)
            {
                var fileName = Path.GetFileName(titleResult.Value.FileName);
                var remotePath = string.Join("/", ["/uploads", fileName]);
                var queueResult = await FtpClientService.UploadFile(titleResult.Value.FileName, remotePath, _ftpClientSettings.Host, _ftpClientSettings.Port);
                if (queueResult.IsSuccess)
                {
                    queuedSuccessCount++;
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