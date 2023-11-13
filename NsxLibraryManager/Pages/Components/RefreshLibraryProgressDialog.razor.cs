using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services;
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

    
    protected override async Task OnInitializedAsync()
    {
        FilesEnumerable = await TitleLibraryService.GetFilesAsync();
        FileCount = FilesEnumerable.Count();
    }
    
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
                    TitleLibraryService.DropLibrary();
                }
                foreach (var file in fileList)
                {
                    var result = await TitleLibraryService.ProcessFileAsync(file);
                    ProgressCompleted++;
                    StateHasChanged();                    
                }

                await TitleLibraryService.AddOwnedDlcToTitlesAsync();
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