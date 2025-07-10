namespace NsxLibraryManager.Shared.Dto;

public class FtpUploadRequest
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FtpHost { get; set; } = string.Empty;
    public int FtpPort { get; set; }
    public string LocalFilePath { get; set; } = string.Empty;
    public string RemotePath { get; set; } = string.Empty;
    public long TotalBytes { get; set; }
    public DateTime Timestamp { get; set; }
}