using System.Collections.Concurrent;
using NsxLibraryManager.Contracts;
using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Services;

public class LibraryBackgroundStateService
{
    private readonly ConcurrentDictionary<string, LibraryBackgroundRequest> _tasks = new();
    private readonly ILogger<LibraryBackgroundStateService> _logger;

    public LibraryBackgroundStateService(ILogger<LibraryBackgroundStateService> logger)
    {
        _logger = logger;
    }

    public void AddTask(LibraryBackgroundRequest task)
    {
        _tasks.TryAdd(task.Id, task);
    }

    public LibraryBackgroundRequest? GetTask(string taskId)
    {
        _tasks.TryGetValue(taskId, out var task);
        return task;
    }

    public void UpdateTaskStatus(string taskId, BackgroundTaskStatus status, string? errorMessage = null)
    {
        if (_tasks.TryGetValue(taskId, out var task))
        {
            task.Status = status;
            task.ErrorMessage = errorMessage;

            if (status == BackgroundTaskStatus.InProgress && !task.StartTime.HasValue)
            {
                task.StartTime = DateTime.Now;
            }
            else if (status == BackgroundTaskStatus.Completed || status == BackgroundTaskStatus.Failed ||
                     status == BackgroundTaskStatus.Cancelled)
            {
                task.CompletionTime = DateTime.Now;
            }

            _logger.LogInformation("Updated task {TaskId} status to {Status}", taskId, status);
        }
    }

    public void UpdateTaskProgress(string taskId, int progress, int totalItems)
    {
        if (_tasks.TryGetValue(taskId, out var task))
        {
            task.Progress = progress;
            task.TotalItems = totalItems;
            _logger.LogDebug("Updated task {TaskId} progress to {Progress}/{TotalItems}", taskId, progress, totalItems);
        }
    }
    public List<LibraryBackgroundRequest> GetAllTasks()
    {
        return _tasks.Values.ToList();
    }

    public List<LibraryBackgroundRequest> GetActiveTasks()
    {
        return _tasks.Values
            .Where(t => t.Status == BackgroundTaskStatus.Queued || t.Status == BackgroundTaskStatus.InProgress)
            .ToList();
    }
    
    public List<LibraryBackgroundRequest> GetActiveTasks(LibraryBackgroundTaskType taskType)
    {
        return _tasks.Values
            .Where(t => t.Status is BackgroundTaskStatus.Queued or BackgroundTaskStatus.InProgress)
            .Where(t => t.TaskType == taskType)
            .ToList();
    }
    
    public List<LibraryBackgroundRequest> GetActiveTasks(LibraryBackgroundTaskType taskType, LibraryBackgroundTaskType taskType2)
    {
        return _tasks.Values
            .Where(t => t.Status is BackgroundTaskStatus.Queued or BackgroundTaskStatus.InProgress)
            .Where(t => t.TaskType == taskType || t.TaskType == taskType2)
            .ToList();
    }
    
    public List<LibraryBackgroundRequest> GetCompletedTasks()
    {
        return _tasks.Values
            .Where(t => t.Status == BackgroundTaskStatus.Completed ||
                        t.Status == BackgroundTaskStatus.Failed ||
                        t.Status == BackgroundTaskStatus.Cancelled)
            .ToList();
    }

    public void CleanupOldTasks(TimeSpan maxAge)
    {
        var cutoffTime = DateTime.Now - maxAge;
        var tasksToRemove = _tasks.Values
            .Where(t => t.CompletionTime.HasValue && t.CompletionTime < cutoffTime)
            .Select(t => t.Id)
            .ToList();

        foreach (var taskId in tasksToRemove)
        {
            _tasks.TryRemove(taskId, out _);
            _logger.LogDebug("Cleaned up old task {TaskId}", taskId);
        }
    }
}