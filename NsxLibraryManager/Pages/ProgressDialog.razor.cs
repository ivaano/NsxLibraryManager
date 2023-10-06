using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services;
using Radzen;

namespace NsxLibraryManager.Pages;

public partial class ProgressDialog : IDisposable
{
    [Inject]
    protected DialogService DialogService { get; set; }
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; }

    public double progressCompleted { get; set; }
    public int fileCount { get; set; }
    
    public IEnumerable<string> fileList { get; set; }

    
    protected override async Task OnInitializedAsync()
    {
        fileList = await TitleLibraryService.GetFilesAsync();
        fileCount = fileList.Count();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await DoWork();
        }
    }
    
    public async Task DoWork()
    {
        await InvokeAsync(
            async () =>
            {
                fileList = await TitleLibraryService.GetFilesAsync();
                fileCount = fileList.Count();
                StateHasChanged();
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
        fileList = new List<string>();
    }
}