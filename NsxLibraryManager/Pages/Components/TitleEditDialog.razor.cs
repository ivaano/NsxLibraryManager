using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Services.Interface;

namespace NsxLibraryManager.Pages.Components;

public partial class TitleEditDialog : ComponentBase
{
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; } = default!;
    
    private LibraryTitleDto title = null!;
    

    void OnSubmit()
    {
        
    }
}