using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using Radzen;

namespace NsxLibraryManager.Pages.Components;

public partial class DeleteLibraryDuplicatesProgressDialog : IDisposable
{
    [Inject]
    protected DialogService DialogService { get; set; }
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; }
    [Inject]
    protected ILogger<RefreshLibraryProgressDialog> Logger { get; set; }
    public double ProgressCompleted { get; set; }
    public int FileCount { get; set; }

    [Parameter] public IList<LibraryTitleDto>? selectedTitles { get; set; }

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
                if (selectedTitles != null)
                {
                    FileCount = selectedTitles.Count;
                    foreach (var title in selectedTitles)
                    {
                        await TitleLibraryService.RemoveDuplicateTitle(title);
                        ProgressCompleted++;
                        StateHasChanged();
                    }
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