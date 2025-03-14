using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Shared.Dto;

public class FtpCompletedTransfer
{
    private bool _success;
    public string TransferId { get; set; }
    public string Filename { get; set; }
    public string LocalFilePath { get; set; }
    public TransferDirection Direction { get; set; }
    public long TotalBytes { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime CompletionTime { get; set; }
    public TimeSpan Duration => CompletionTime - StartTime;
}