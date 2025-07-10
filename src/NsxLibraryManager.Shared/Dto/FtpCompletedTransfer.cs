using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Shared.Dto;

public class FtpCompletedTransfer
{
    //private bool _success;
    public string TransferId { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public string LocalFilePath { get; set; } = string.Empty;
    public TransferDirection Direction { get; set; }
    public long TotalBytes { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime CompletionTime { get; set; }
    public TimeSpan Duration => CompletionTime - StartTime;
}