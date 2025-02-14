namespace NsxLibraryManager.Models;

public class FileUploadResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}