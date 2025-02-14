using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services.Interface;
using Radzen;

namespace NsxLibraryManager.Pages.Components;
#nullable disable
public partial class RefreshLibraryProgressDialog : IDisposable
{
    [Inject]
    protected DialogService DialogService { get; set; }
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; }
    [Inject]
    protected ILogger<RefreshLibraryProgressDialog> Logger { get; set; }
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
                    var result = await TitleLibraryService.AddLibraryTitleAsync(libraryTitle);
                    ProgressCompleted++;
                    StateHasChanged();
                }

                foreach (var libraryTitle in filesToProcess.FilesToRemove)
                {
                    var result = await TitleLibraryService.RemoveLibraryTitleAsync(libraryTitle);
                    ProgressCompleted++;
                    StateHasChanged();
                }
                
                foreach (var libraryTitle in filesToProcess.FilesToUpdate)
                {
                    var result = await TitleLibraryService.UpdateLibraryTitleAsync(libraryTitle);
                    ProgressCompleted++;
                    StateHasChanged();
                }
                await TitleLibraryService.SaveLibraryReloadDate(refresh: true);


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