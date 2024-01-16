using Microsoft.AspNetCore.Components;

namespace NsxLibraryManager.Shared;

public partial class NavMenu
{
    [Inject] private IConfiguration Configuration { get; set; } = default!;
    
    private bool _initialConfig = true;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        if (Configuration.GetValue<string>("IsDefaultConfigCreated") == "True")
        {
            _initialConfig = false;
        }
    }
}