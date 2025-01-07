using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Core.Services.Interface;
using Radzen;


namespace NsxLibraryManager.Pages.Components;

public partial class RefreshPatchesProgressDialog
{
    
    [Inject]
    protected DialogService DialogService { get; set; } = default!;
    //[Inject]
    //protected ITitleLibraryService TitleLibraryService { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        /*
        if (firstRender)
        {
            await TitleLibraryService.ProcessAllTitlesUpdates();
            DialogService.Close();
        }
        */
    }
}