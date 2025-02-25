using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Shared.Dto;

public class FtpUploadStatus
{
    public string Id { get; set; }
    public UploadStatusType Status { get; set; }
    public string FileName { get; set; }
    public double Progress { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? CompletionTime { get; set; }
    public string ErrorMessage { get; set; }
}