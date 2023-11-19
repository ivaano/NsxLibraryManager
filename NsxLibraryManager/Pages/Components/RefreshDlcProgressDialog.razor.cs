using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Core.Services.Interface;
using Radzen;


namespace NsxLibraryManager.Pages.Components;

partial class RefreshDlcProgressDialog
{
    [Inject]
    protected DialogService DialogService { get; set; }
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await TitleLibraryService.AddOwnedDlcToTitlesAsync();
            DialogService.Close();
        }
        
    }
}