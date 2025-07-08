using System.Collections.Concurrent;
using NsxLibraryManager.Contracts;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Services;

public class LibraryBackgroundService : BackgroundService
{
    private readonly ILogger<LibraryBackgroundService> _logger;
    private readonly ConcurrentQueue<LibraryBackgroundRequest> _taskQueue = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly LibraryBackgroundStateService _stateService;
    private readonly IServiceProvider _serviceProvider;

    public LibraryBackgroundService(
        ILogger<LibraryBackgroundService> logger,
        LibraryBackgroundStateService stateService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _stateService = stateService;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessTaskQueue(stoppingToken);

                await Task.WhenAny(
                    Task.Delay(5000, stoppingToken),
                    _signal.WaitAsync(stoppingToken));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during library refresh operation");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    public string QueueSingleRunLibraryRequest(LibraryBackgroundTaskType taskType, Dictionary<string, object>? parameters = null)
    {
        var queuedOrInProgress = _stateService.IsTaskTypeQueuedOrInProgress(taskType);
        return queuedOrInProgress.Count > 0 ? 
            queuedOrInProgress.First().Id : 
            QueueLibraryRequest(LibraryBackgroundTaskType.Refresh);
    }

    public string QueueLibraryRequest(LibraryBackgroundTaskType taskType, Dictionary<string, object>? parameters = null)
    {
        var request = new LibraryBackgroundRequest
        {
            Id = Guid.NewGuid().ToString(),
            TaskType = taskType,
            Parameters = parameters ?? new Dictionary<string, object>(),
            Timestamp = DateTime.Now,
            Status = BackgroundTaskStatus.Queued
        };
        
        try
        {
            _taskQueue.Enqueue(request);
            _stateService.AddTask(request);
            _signal.Release();
            _logger.LogInformation("Queued library refresh request: {taskType}, ID: {id}", taskType, request.Id);
            return request.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queuing library refresh request: {taskType}", taskType);
            throw;
        }
    }

    public bool RemoveQueuedTask(string requestId)
    {
        if (string.IsNullOrEmpty(requestId))
        {
            _logger.LogWarning("Attempted to remove queued task with null or empty ID");
            return false;
        }

        var found = false;
        var tempQueue = new ConcurrentQueue<LibraryBackgroundRequest>();
    
        while (_taskQueue.TryDequeue(out var request))
        {
            if (request.Id != requestId)
            {
                tempQueue.Enqueue(request);
            }
            else
            {
                found = true;
                _stateService.UpdateTaskStatus(requestId, BackgroundTaskStatus.Cancelled);
                _logger.LogInformation("Removed queued library refresh request: {taskType}, ID: {id}", 
                    request.TaskType, request.Id);
            }
        }
    
        while (tempQueue.TryDequeue(out var request))
        {
            _taskQueue.Enqueue(request);
        }
    
        if (!_taskQueue.IsEmpty && found)
        {
            _signal.Release();
        }
    
        return found;
    }

    public List<LibraryBackgroundRequest> GetTaskQueue()
    {
        return _taskQueue.ToList();
    }
    
    public LibraryBackgroundStatus GetTaskStatus(string id)
    {
        var task = _stateService.GetTask(id);
        if (task != null)
        {
            return new LibraryBackgroundStatus
            {
                Id = id,
                Status = task.Status,
                TaskType = task.TaskType,
                StartTime = task.StartTime,
                CompletionTime = task.CompletionTime,
                Progress = task.Progress,
                TotalItems = task.TotalItems,
                ErrorMessage = task.ErrorMessage
            };
        }
        
        return new LibraryBackgroundStatus
        {
            Id = id,
            Status = BackgroundTaskStatus.NotFound
        };
    }
    
    private async Task ProcessTaskQueue(CancellationToken stoppingToken)
    {
        while (_taskQueue.TryDequeue(out var request) && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessLibraryTask(request, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing library refresh task: {id}", request.Id);
                
                _stateService.UpdateTaskStatus(request.Id, BackgroundTaskStatus.Failed, ex.Message);
            }
        }
    }
    
    private async Task ProcessLibraryTask(LibraryBackgroundRequest request, CancellationToken stoppingToken)
    {
        _stateService.UpdateTaskStatus(request.Id, BackgroundTaskStatus.InProgress);
        
        try
        {
            _logger.LogInformation("Starting library refresh task: {TaskType}, ID: {Id}", 
                request.TaskType, request.Id);

            using var scope = _serviceProvider.CreateScope();
            var titleLibraryService = scope.ServiceProvider.GetRequiredService<ITitleLibraryService>();

            switch (request.TaskType)
            {
                case LibraryBackgroundTaskType.Refresh:
                    await ProcessDeltaRefresh(request, titleLibraryService, stoppingToken);
                    break;
                default:
                    throw new ArgumentException($"Unknown task type: {request.TaskType}");
            }

            _stateService.UpdateTaskStatus(request.Id, BackgroundTaskStatus.Completed);
            _logger.LogInformation("Completed library refresh task: {TaskType}, ID: {Id}", 
                request.TaskType, request.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing library refresh task: {id}", request.Id);
            _stateService.UpdateTaskStatus(request.Id, BackgroundTaskStatus.Failed, ex.Message);
            throw;
        }
    }

    private async Task ProcessDeltaRefresh(LibraryBackgroundRequest request, ITitleLibraryService titleLibraryService, CancellationToken stoppingToken)
    {
        var filesToProcess = await titleLibraryService.GetDeltaFilesInLibraryAsync();
        var totalFiles = filesToProcess.TotalFiles;
        var processedFiles = 0;

        _stateService.UpdateTaskProgress(request.Id, 0, totalFiles);

        foreach (var libraryTitle in filesToProcess.FilesToAdd)
        {
            if (stoppingToken.IsCancellationRequested) break;
            
            await titleLibraryService.AddLibraryTitleAsync(libraryTitle);
            processedFiles++;
            _stateService.UpdateTaskProgress(request.Id, processedFiles, totalFiles);
        }

        foreach (var libraryTitle in filesToProcess.FilesToRemove)
        {
            if (stoppingToken.IsCancellationRequested) break;
            
            await titleLibraryService.RemoveLibraryTitleAsync(libraryTitle);
            processedFiles++;
            _stateService.UpdateTaskProgress(request.Id, processedFiles, totalFiles);
        }
        
        foreach (var libraryTitle in filesToProcess.FilesToUpdate)
        {
            if (stoppingToken.IsCancellationRequested) break;
            
            await titleLibraryService.UpdateLibraryTitleAsync(libraryTitle);
            processedFiles++;
            _stateService.UpdateTaskProgress(request.Id, processedFiles, totalFiles);
        }
        
        await titleLibraryService.SaveLibraryReloadDate(refresh: true);

        using var scope = _serviceProvider.CreateScope();
        var webhookService = scope.ServiceProvider.GetRequiredService<IWebhookService>();
        var payload = new { EventType = nameof(WebhookType.LibraryRefresh), TimeStamp = DateTime.Now };
        await webhookService.SendWebhook(WebhookType.LibraryRefresh, payload);
    }

}