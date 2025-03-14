using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class FtpStatus : ComponentBase, IDisposable
{
    RadzenDataGrid<FtpStatusGridDto> grid;
    
    [Inject]
    private FtpStateService FtpStateService { get; set; } = null!;
    
    [Inject] 
    protected IFtpClientService FtpClientService { get; set; } = default;

    private IEnumerable<FtpStatusGridDto> data { get; set; }
    
    private System.Timers.Timer _refreshTimer;
    private bool _disposed = false;

    private async Task RefreshData()
    {
        var currentTransfers = FtpStateService.CurrentTransfers;

        var gridData = currentTransfers.Select(c => new FtpStatusGridDto
        {
            TransferId = c.TransferId,
            Filename = c.Filename,
            Direction = c.Direction,
            TotalBytes = c.TotalBytes,
            TransferredBytes = c.TransferredBytes,
            Progress = c.Progress,
            ProgressPercentage = c.ProgressPercentage,
            StartTime = c.StartTime,
            LastUpdateTime = c.LastUpdateTime,
        }).ToList();
        
        var queuedFiles = await FtpClientService.GetQueuedFiles();
        if (queuedFiles.IsSuccess)
        {
            gridData.AddRange(queuedFiles.Value.Select(request => new FtpStatusGridDto
            {
                TransferId = request.Id,
                Filename = request.FileName,
                Direction = TransferDirection.Upload, 
                TotalBytes = request.TotalBytes, 
                TransferredBytes = 0,
                Progress = 0,
                ProgressPercentage = 0,
                LocalFilePath = request.LocalFilePath,
                RemotePath = request.RemotePath,
                Timestamp = request.Timestamp,
                StartTime = request.Timestamp,
                LastUpdateTime = request.Timestamp, 
                FtpHost = request.FtpHost,
                FtpPort = request.FtpPort
            }));
        }

        data = gridData.AsEnumerable();
    }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        await RefreshData();       
        _refreshTimer = new System.Timers.Timer(1000);
        _refreshTimer.Elapsed += async (s, e) =>
        {
            await InvokeAsync(async () =>
            {
                await RefreshData();
                await grid.Reload();
                StateHasChanged();
            });
        };

        _refreshTimer.Start();
    }

    private async Task DeleteUpload(string transferId)
    {
        var deleteResult = await FtpClientService.RemoveQueuedFileUpload(transferId);
    }

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