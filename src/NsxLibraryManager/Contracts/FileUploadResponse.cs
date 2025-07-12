namespace NsxLibraryManager.Contracts;

public class FileUploadResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; }  = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}