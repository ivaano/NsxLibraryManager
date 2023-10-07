using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services;
using Radzen;

namespace NsxLibraryManager.Pages.Components;

public partial class RefreshLibraryProgressDialog : IDisposable
{
    [Inject]
    protected DialogService DialogService { get; set; }
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; }

    public double progressCompleted { get; set; }
    public int fileCount { get; set; }

    private IEnumerable<string> FilesEnumerable { get; set; }

    
    protected override async Task OnInitializedAsync()
    {
        FilesEnumerable = await TitleLibraryService.GetFilesAsync();
        fileCount = FilesEnumerable.Count();
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
                fileCount = fileList.Count;
                if (fileCount > 0)
                {
                    TitleLibraryService.DropLibraryAsync();
                }
                foreach (var file in fileList)
                {
                    var result = await TitleLibraryService.ProcessFileAsync(file);
                    progressCompleted++;
                    StateHasChanged();                    
                }
            });
        DialogService.Close();
    }
    
    void IDisposable.Dispose()
    {
        fileCount = 0;
        progressCompleted = 0;
        FilesEnumerable = new List<string>();
    }
}