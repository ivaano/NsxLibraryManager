using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Enums;
using Radzen;

namespace NsxLibraryManager.Pages.Components;
#nullable disable
public partial class RefreshLibraryProgressDialog : IDisposable
{
    [Inject] protected DialogService DialogService { get; set; }
    [Inject] protected ITitleLibraryService TitleLibraryService { get; set; }
    [Inject] protected IWebhookService WebhookService { get; set; }    

    public double ProgressCompleted { get; set; }
    public int FileCount { get; set; }

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
                }

                foreach (var libraryTitle in filesToProcess.FilesToRemove)
                {
                    await TitleLibraryService.RemoveLibraryTitleAsync(libraryTitle);
                    ProgressCompleted++;
                    StateHasChanged();
                }
                
                foreach (var libraryTitle in filesToProcess.FilesToUpdate)
                {
                    await TitleLibraryService.UpdateLibraryTitleAsync(libraryTitle);
                    ProgressCompleted++;
                    StateHasChanged();
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