using Microsoft.AspNetCore.Components;
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
                foreach (var file in fileList)
                {
                    var result = await TitleLibraryService.ProcessFileAsync(file);
                    ProgressCompleted++;
                    StateHasChanged();                    
                }

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