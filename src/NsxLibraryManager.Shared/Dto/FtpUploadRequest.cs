namespace NsxLibraryManager.Shared.Dto;

public class FtpUploadRequest
{
    public string Id { get; set; }
    public string FileName { get; set; }
    public string FtpHost { get; set; }
    public int FtpPort { get; set; }
    public string LocalFilePath { get; set; }
    public string RemotePath { get; set; }
    public long TotalBytes { get; set; }
    public DateTime Timestamp { get; set; }
}