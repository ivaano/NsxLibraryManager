using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Contracts;
using NsxLibraryManager.Services;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class TaskStatus : ComponentBase, IDisposable
{
    [Inject]
    private LibraryBackgroundStateService  LibraryBackgroundStateService { get; set; } = null!;
    
    [Inject]
    private TooltipService TooltipService { get; set; } = null!;
    
    private IEnumerable<LibraryBackgroundRequest> data { get; set; } = Enumerable.Empty<LibraryBackgroundRequest>();
    private RadzenDataGrid<LibraryBackgroundRequest>? grid;
    private System.Timers.Timer? _refreshTimer;
    private bool _disposed;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        await RefreshData();       
        _refreshTimer = new System.Timers.Timer(1000);
        _refreshTimer.Elapsed += async (_, _) =>
        {
            await InvokeAsync(async () =>
            {
                await RefreshData();
                if (grid is not null) await grid.Reload();
                StateHasChanged();
            });
        };

        _refreshTimer.Start();
    }
    
    private Task RefreshData()
    {
        var activeTasks = LibraryBackgroundStateService.GetAllTasks();
        var gridData = activeTasks.ToList();
        data = gridData.AsEnumerable();
        return Task.CompletedTask;
    }

    private void CleanupTasks()
    {
        LibraryBackgroundStateService.CleanupOldTasks(TimeSpan.FromHours(3));
    }

    private void ShowTooltip(ElementReference elementReference, TooltipOptions? options = null) => TooltipService.Open(elementReference, "Remove tasks older than 24 hours.", options);
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
        }

        _disposed = true;
    }
}