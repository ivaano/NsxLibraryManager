using Microsoft.AspNetCore.Components;
using Radzen;

namespace NsxLibraryManager.Shared;

public partial class NavMenu
{
    [Inject] private IConfiguration Configuration { get; set; } = default!;
    [Inject] private TooltipService tooltipService { get; set; } = default!;
    
    private bool _initialConfig = true;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        if (Configuration.GetValue<string>("IsDefaultConfigCreated") == "True")
        {
            _initialConfig = false;
        }
    }
    
    private void ShowTooltip(ElementReference elementReference, string message)
    {
        var options = new TooltipOptions
        {
            Position = TooltipPosition.Bottom,
            Duration = 3000
        };
        tooltipService.Open(elementReference, message, options);
    }
}