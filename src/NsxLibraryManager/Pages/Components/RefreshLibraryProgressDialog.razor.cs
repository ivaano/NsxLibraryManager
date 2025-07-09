using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Enums;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages.Components;
#nullable disable
public partial class RefreshLibraryProgressDialog : IDisposable
{
    [Inject] private DialogService DialogService { get; set; }
    [Inject] private ITitleLibraryService TitleLibraryService { get; set; }
    [Inject] private IWebhookService WebhookService { get; set; }
    
    [Inject] private LibraryBackgroundStateService  LibraryBackgroundStateService { get; set; }
    
    public double ProgressCompleted { get; set; }
    public int FileCount { get; set; }
    [Parameter] public string RequestId { get; set; }

    private IEnumerable<string> FilesEnumerable { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await DoWork();
        }
    }

    private async Task DoWork()
    {
        await InvokeAsync(
            async () =>
            {
                var filesToProcess = await TitleLibraryService.GetDeltaFilesInLibraryAsync();
                FileCount = filesToProcess.TotalFiles;
                foreach (var libraryTitle in filesToProcess.FilesToAdd)
                {
                    await TitleLibraryService.AddLibraryTitleAsync(libraryTitle);
                    ProgressCompleted++;
                    StateHasChanged();
                    LibraryBackgroundStateService.UpdateTaskProgress(RequestId, (int)ProgressCompleted, FileCount);
                }

                foreach (var libraryTitle in filesToProcess.FilesToRemove)
                {
                    await TitleLibraryService.RemoveLibraryTitleAsync(libraryTitle);
                    ProgressCompleted++;
                    StateHasChanged();
                    LibraryBackgroundStateService.UpdateTaskProgress(RequestId, (int)ProgressCompleted, FileCount);
                }
                
                foreach (var libraryTitle in filesToProcess.FilesToUpdate)
                {
                    await TitleLibraryService.UpdateLibraryTitleAsync(libraryTitle);
                    ProgressCompleted++;
                    StateHasChanged();
                    LibraryBackgroundStateService.UpdateTaskProgress(RequestId, (int)ProgressCompleted, FileCount);
                }
                await TitleLibraryService.SaveLibraryReloadDate(refresh: true);


            });
        var payload = new { EventType = nameof(WebhookType.LibraryRefresh), TimeStamp = DateTime.UtcNow };
        await WebhookService.SendWebhook(WebhookType.LibraryRefresh, payload);
        DialogService.Close();
    }
    
    void IDisposable.Dispose()
    {
        FileCount = 0;
        ProgressCompleted = 0;
        FilesEnumerable = new List<string>();
    }
}