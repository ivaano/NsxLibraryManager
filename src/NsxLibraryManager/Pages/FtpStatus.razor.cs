using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class FtpStatus : ComponentBase
{
    RadzenDataGrid<IDictionary<string, object>> grid;
    
    [Inject]
    private FtpStateService FtpStateService { get; set; } = null!;
    
    [Inject] 
    protected IFtpClientService FtpClientService { get; set; } = default;
    
    public IEnumerable<IDictionary<string, object>> data 
    { 
        get
        {
            return Enumerable.Range(0, 20).Select(i =>
            {
                var row = new Dictionary<string, object>();

                foreach (var column in columns)
                {
                    row.Add(
                        column.Key,
                        column.Value == typeof(Guid) ? Guid.NewGuid()
                        : column.Value == typeof(DateTime) ? DateTime.Now.AddMonths(i)
                        : $"{column.Key}{i}"
                    );
                }

                return row;
            });
        }
    }
    
    public IDictionary<string, Type> columns { get; set; }

    
    
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        FtpStateService.OnStatusUpdate += HandleFtpStatus;

        var taako = FtpStateService.CurrentTransfers;
        columns = new Dictionary<string, Type>()
        {
            { "ID", typeof(Guid) },
            { "Date", typeof(DateTime) },
        };

        var timer = new System.Timers.Timer(1000);
        timer.Elapsed += (s, e) =>
        {
            InvokeAsync(grid.Reload);
        };

        timer.Start();
    }
    
    private void HandleFtpStatus(FtpStatusUpdate status)
    {
        
    }
}