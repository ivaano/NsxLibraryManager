using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Shared.Dto;

public class FtpStatusGridDto
{
    public string TransferId { get; set; }
    public string Filename { get; set; }
    public string FtpHost { get; set; }
    public int FtpPort { get; set; }
    public TransferDirection Direction { get; set; }
    public long TotalBytes { get; set; }
    public long TransferredBytes { get; set; }
    public double Progress { get; set; }
    public double ProgressPercentage { get; set; } 
    public string LocalFilePath { get; set; }
    public string RemotePath { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime LastUpdateTime { get; set; }
}