using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Shared.Dto;

namespace NsxLibraryManager.Pages.Components;

public partial class TitleMassEditDialog : ComponentBase
{
    [Parameter]
    public IList<LibraryTitleDto> SelectedTitles { get; set; } = null!;
    
    
    protected override async Task OnParametersSetAsync()
    {
        var hola = false;
    }
}