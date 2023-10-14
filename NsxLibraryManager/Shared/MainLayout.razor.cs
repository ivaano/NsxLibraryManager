using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace NsxLibraryManager.Shared;
#nullable disable

public partial class MainLayout
{
    [Inject]
    protected IJSRuntime JSRuntime { get; set; }

    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    [Inject]
    protected DialogService DialogService { get; set; }

    [Inject]
    protected TooltipService TooltipService { get; set; }

    [Inject]
    protected ContextMenuService ContextMenuService { get; set; }

    [Inject]
    protected NotificationService NotificationService { get; set; }

    private bool sidebarExpanded = true;

    void SidebarToggleClick()
    {
        sidebarExpanded = !sidebarExpanded;
    }
}