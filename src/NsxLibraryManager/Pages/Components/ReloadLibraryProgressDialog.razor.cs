using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using Radzen;

namespace NsxLibraryManager.Pages.Components;
#nullable disable
public partial class ReloadLibraryProgressDialog : IDisposable
{
    [Inject]
    protected DialogService DialogService { get; set; }
    
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; }
    
    [Inject]
    protected IWebhookService WebhookService { get; set; }
    
    [Inject] private LibraryBackgroundStateService  LibraryBackgroundStateService { get; set; }


    private double ProgressCompleted { get; set; }
    private int FileCount { get; set; }
    [Parameter] public string RequestId { get; set; }

    private IEnumerable<LibraryFileDto> FilesEnumerable { get; set; }

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
                FilesEnumerable = await TitleLibraryService.GetLibraryFilesAsync();
                var fileList = FilesEnumerable.ToList();
                FileCount = fileList.Count;
                if (FileCount > 0)
                {
                    await TitleLibraryService.DropLibrary();
                }

                var updateCounts = new Dictionary<string, int>();
                var dlcCounts = new Dictionary<string, int>();
                foreach (var file in fileList)
                {
                    var title = await TitleLibraryService.ProcessFileAsync(file);

                    if (title is { ContentType: TitleContentType.Update, OtherApplicationId: not null })
                    {
                        updateCounts[title.OtherApplicationId] = updateCounts.GetValueOrDefault(title.OtherApplicationId) + 1;
                    }
                    
                    if (title is { ContentType: TitleContentType.DLC, OtherApplicationId: not null })
                    {
                        dlcCounts[title.OtherApplicationId] = dlcCounts.GetValueOrDefault(title.OtherApplicationId) + 1;
                    }
                    
                    ProgressCompleted++;
                    LibraryBackgroundStateService.UpdateTaskProgress(RequestId, (int)ProgressCompleted, FileCount);
                    StateHasChanged();                    
                }
      
                await TitleLibraryService.SaveContentCounts(updateCounts, TitleContentType.Update);
                await TitleLibraryService.SaveContentCounts(dlcCounts, TitleContentType.DLC);
                
                await TitleLibraryService.SaveLibraryReloadDate();
            });
        var payload = new { EventType = nameof(WebhookType.LibraryReload), TimeStamp = DateTime.Now };
        await WebhookService.SendWebhook(WebhookType.LibraryReload, payload);
        DialogService.Close();
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
        FileCount = 0;
        ProgressCompleted = 0;
        FilesEnumerable = new List<LibraryFileDto>();
        if (TitleLibraryService is not null)
        {
            TitleLibraryService = null;
        }
    }
}