using System.Collections.Concurrent;
using FluentFTP;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Services;

public class FtpBackgroundService : BackgroundService
{
    private readonly ILogger<FtpBackgroundService> _logger;

    private readonly ConcurrentQueue<FtpUploadRequest> _uploadQueue = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly FtpStateService _stateService;

    public FtpBackgroundService(
        ILogger<FtpBackgroundService> logger,
        FtpStateService stateService)
    {
        _logger = logger;
        _stateService = stateService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessUploadQueue(stoppingToken);

                await Task.WhenAny(
                    Task.Delay(5000, stoppingToken),
                    _signal.WaitAsync(stoppingToken));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown, don't log an error
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ocured during FTP operation");
                await Task.Delay(5000, stoppingToken); // Wait a bit before retrying after error
            }
        }
    }
    
    public string QueueFileUpload(string filePath, string remotePath, string ftpHost, int ftpPort)
    {
        var  fileInfo = new FileInfo(filePath);
        var request = new FtpUploadRequest
        {
            Id = Guid.NewGuid().ToString(),
            FileName = fileInfo.Name,
            LocalFilePath = filePath,
            TotalBytes = fileInfo.Length,
            RemotePath = remotePath,
            FtpHost = ftpHost,
            FtpPort = ftpPort,
            Timestamp = DateTime.Now
        };
        
       
        try
        {
            _uploadQueue.Enqueue(request);
            _signal.Release();
            _logger.LogInformation("Queued file upload: {fileName}, ID: {id}", filePath, request.Id);
            return request.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queuing file for upload: {fileName}", filePath);
            throw;
        }
    }

    public bool RemoveQueuedFileUpload(string queuedFileId)
    {
        if (string.IsNullOrEmpty(queuedFileId))
        {
            _logger.LogWarning("Attempted to remove queued file with null or empty ID");
            return false;
        }

        var found = false;
        var tempQueue = new ConcurrentQueue<FtpUploadRequest>();
    
        while (_uploadQueue.TryDequeue(out var request))
        {
            if (request.Id != queuedFileId)
            {
                tempQueue.Enqueue(request);
            }
            else
            {
                found = true;
                _logger.LogInformation("Removed queued file upload: {fileName}, ID: {id}", 
                    request.FileName, request.Id);
            }
        }
    
        while (tempQueue.TryDequeue(out var request))
        {
            _uploadQueue.Enqueue(request);
        }
    
        if (!_uploadQueue.IsEmpty && found)
        {
            _signal.Release();
        }
    
        return found;
    }

    public List<FtpUploadRequest> GetUploadQueue()
    {
        return _uploadQueue.ToList();
    }
    
    public FtpUploadStatus GetUploadStatus(string id)
    {
        // Check if it's in the state service (in progress or completed)
        var activeTransfer = _stateService.CurrentTransfers
            .FirstOrDefault(t => t.TransferId == id);
            
        if (activeTransfer != null)
        {
            return new FtpUploadStatus
            {
                Id = id,
                Status = UploadStatusType.InProgress,
                FileName = activeTransfer.Filename,
                Progress = activeTransfer.ProgressPercentage,
                StartTime = activeTransfer.StartTime
            };
        }
        
        var completedTransfer = _stateService.CompletedTransfers
            .FirstOrDefault(t => t.TransferId == id);
            
        if (completedTransfer != null)
        {
            return new FtpUploadStatus
            {
                Id = id,
                Status = completedTransfer.Success ? 
                UploadStatusType.Completed : UploadStatusType.Failed,
                FileName = completedTransfer.Filename,
                Progress = completedTransfer.Success ? 100 : 0,
                StartTime = completedTransfer.StartTime,
                CompletionTime = completedTransfer.CompletionTime,
                ErrorMessage = completedTransfer.ErrorMessage
            };
        }
        
        // Check if it's still in the queue
        if (_uploadQueue.Any(r => r.Id == id))
        {
            return new FtpUploadStatus
            {
                Id = id,
                Status = UploadStatusType.Queued,
                FileName = _uploadQueue.First(r => r.Id == id).FileName
            };
        }
        
        return new FtpUploadStatus
        {
            Id = id,
            Status = UploadStatusType.NotFound
        };
    }
    
    private async Task ProcessUploadQueue(CancellationToken stoppingToken)
    {
        while (_uploadQueue.TryDequeue(out var request) && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UploadFile(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing upload request: {id}", request.Id);
                
                // Report failure
                _stateService.CompleteTransfer(new FtpCompletedTransfer
                {
                    TransferId = request.Id,
                    Filename = request.FileName,
                    Direction = TransferDirection.Upload,
                    Success = false,
                    ErrorMessage = ex.Message,
                    StartTime = request.Timestamp,
                    CompletionTime = DateTime.Now
                });
            }
            finally
            {
                try
                {

                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to clean up temporary file: {path}", request.LocalFilePath);
                }
            }
        }
    }
    
    private async Task UploadFile(FtpUploadRequest request)
    {
        if (!File.Exists(request.LocalFilePath))
        {
            throw new FileNotFoundException("File for upload was not found", request.LocalFilePath);
        }
        
        
        // Start the transfer status
        _stateService.UpdateStatus(new FtpStatusUpdate
        {
            TransferId = request.Id,
            Filename = request.FileName,
            LocalFilePath = request.LocalFilePath,
            Direction = TransferDirection.Upload,
            TotalBytes = request.TotalBytes,
            TransferredBytes = 0,
            StartTime = DateTime.Now,
            LastUpdateTime = DateTime.Now
        });


        using var ftp = new FtpClient(request.FtpHost, request.FtpPort);
        ftp.Connect();

        try
        {
            // upload a file with progress tracking
            _logger.LogInformation("Starting file upload: {FileName}, ID: {Id}", request.FileName, request.Id);

            var result = ftp.UploadFile(request.LocalFilePath, request.RemotePath, FtpRemoteExists.Overwrite, false,
                FtpVerify.None, Progress);


            _stateService.CompleteTransfer(new FtpCompletedTransfer
            {
                TransferId = request.Id,
                Filename = request.FileName,
                LocalFilePath = request.LocalFilePath,
                Direction = TransferDirection.Upload,
                TotalBytes = request.TotalBytes,
                Success = result == FtpStatus.Success,
                StartTime = request.Timestamp,
                CompletionTime = DateTime.Now
            });

            _logger.LogInformation("Completed upload of file: {fileName}, result: {result}",
                request.FileName, result);
        }
        finally
        {
            if (ftp.IsConnected)
            {
                ftp.Disconnect();
            }
        }

        return;

        // define the progress tracking callback
        void Progress(FtpProgress p)
        {
            if (p.Progress == 1)
            {
                // all done!
            }
            else
            {
                _stateService.UpdateStatus(new FtpStatusUpdate
                {
                    TransferId = request.Id,
                    Filename = request.FileName,
                    LocalFilePath = request.LocalFilePath,
                    TotalBytes = request.TotalBytes,
                    Direction = TransferDirection.Upload,
                    Progress = p.Progress,
                    TransferredBytes = p.TransferredBytes,
                    StartTime = request.Timestamp,
                    LastUpdateTime = DateTime.Now
                });
            }
        }
    }
}