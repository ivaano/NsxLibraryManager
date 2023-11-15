using NsxLibraryManager.Services.Interface;
using Microsoft.AspNetCore.Components;
using Radzen;


namespace NsxLibraryManager.Pages.Components;

public partial class RefreshPatchesProgressDialog
{
    
    [Inject]
    protected DialogService DialogService { get; set; }
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await TitleLibraryService.AddOwnedUpdateToTitlesAsync();
            DialogService.Close();
        }
        
    }
}