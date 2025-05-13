using System.Reflection;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace NsxLibraryManager.Shared;

public partial class NavMenu
{
    [Inject] private IConfiguration Configuration { get; set; } = default!;
    [Inject] private TooltipService tooltipService { get; set; } = default!;
    
    private bool _initialConfig = true;
    private string _version = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        if (version is not null) _version = $"version {version.Major}.{version.Minor}.{version.Build}";

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