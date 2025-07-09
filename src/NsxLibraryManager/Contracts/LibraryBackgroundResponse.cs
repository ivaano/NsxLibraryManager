namespace NsxLibraryManager.Contracts;

public class LibraryBackgroundResponse
{
    public string? TaskId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; } 
}