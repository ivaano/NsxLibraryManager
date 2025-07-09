using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Contracts;

public class LibraryBackgroundRequest
{
    public string Id { get; set; } = string.Empty;
    public LibraryBackgroundTaskType TaskType { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public BackgroundTaskStatus Status { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? CompletionTime { get; set; }
    public int Progress { get; set; }
    public int TotalItems { get; set; }
    public string? ErrorMessage { get; set; }
}