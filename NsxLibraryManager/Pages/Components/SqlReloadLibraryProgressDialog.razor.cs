using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Services.Interface;
using Radzen;

namespace NsxLibraryManager.Pages.Components;
#nullable disable
public partial class SqlReloadLibraryProgressDialog : IDisposable
{
    [Inject]
    protected DialogService DialogService { get; set; }
    [Inject]
    protected ISqlTitleLibraryService TitleLibraryService { get; set; }
    [Inject]
    protected ILogger<SqlReloadLibraryProgressDialog> Logger { get; set; }
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
                FilesEnumerable = await TitleLibraryService.GetFilesAsync();
                var fileList = FilesEnumerable.ToList();
                FileCount = fileList.Count;
                if (FileCount > 0)
                {
                    await TitleLibraryService.DropLibrary();
                }

                var batchCount = 0;
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
                    
                    batchCount++;
                    if (batchCount >= 100)
                    {
                        await TitleLibraryService.SaveDatabaseChangesAsync();
                        batchCount = 0;
                    }
                    ProgressCompleted++;
                    StateHasChanged();                    
                }

                if (batchCount > 0)
                {
                    await TitleLibraryService.SaveDatabaseChangesAsync();
                }
                
                await TitleLibraryService.SaveContentCounts(updateCounts, TitleContentType.Update);
                await TitleLibraryService.SaveContentCounts(dlcCounts, TitleContentType.DLC);

            });
        DialogService.Close();
    }
    
    void IDisposable.Dispose()
    {
        FileCount = 0;
        ProgressCompleted = 0;
        FilesEnumerable = new List<string>();
    }
}