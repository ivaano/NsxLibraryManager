using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Shared.Dto;

public class FtpStatusUpdate
{
    public string TransferId { get; set; }
    public string Filename { get; set; }
    public string LocalFilePath { get; set; }
    public TransferDirection Direction { get; set; }
    public long TotalBytes { get; set; }
    public long TransferredBytes { get; set; }
    public double Progress { get; set; }
    public double ProgressPercentage => TotalBytes > 0 ? (double)TransferredBytes / TotalBytes * 100 : 0;
    public DateTime StartTime { get; set; }
    public DateTime LastUpdateTime { get; set; }
}