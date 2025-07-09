using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Contracts;

public class LibraryBackgroundStatus
{
    public string Id { get; set; } = string.Empty;
    public BackgroundTaskStatus Status { get; set; }
    public LibraryBackgroundTaskType TaskType { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? CompletionTime { get; set; }
    public int Progress { get; set; }
    public int TotalItems { get; set; }
    public string? ErrorMessage { get; set; }
    public double ProgressPercentage => TotalItems > 0 ? (double)Progress / TotalItems * 100 : 0;
}