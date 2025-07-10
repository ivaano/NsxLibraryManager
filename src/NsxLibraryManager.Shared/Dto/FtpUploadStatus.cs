using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Shared.Dto;

public class FtpUploadStatus
{
    public string Id { get; set; } = string.Empty;
    public BackgroundTaskStatus Status { get; set; }
    public string FileName { get; set; } = string.Empty;
    public double Progress { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? CompletionTime { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}