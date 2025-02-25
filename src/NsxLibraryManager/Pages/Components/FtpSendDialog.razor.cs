using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using Radzen;

namespace NsxLibraryManager.Pages.Components;

public partial class FtpSendDialog : ComponentBase
{
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; } = default!;

    [Inject] protected IFtpClientService FtpClientService { get; set; } = default;
    
    [Inject]
    protected DialogService DialogService { get; set; } = default!;
    
    
    [Parameter]
    public IList<LibraryTitleDto> SelectedTitles { get; set; } = null!;

    private async Task OnSubmit()
    {
        foreach (var title in SelectedTitles)
        {
            var titleResult = await TitleLibraryService.GetTitleByApplicationId(title.ApplicationId);
            if (titleResult.Value.FileName is not null)
            {
                var response = FtpClientService.UploadFile(titleResult.Value.FileName, "/");
            }
        }
        //var titleResult = await TitleLibraryService.GetTitleByApplicationId(TitleId);
    }
    
    private void Cancel()
    {
        DialogService.Close(false);
    }
}