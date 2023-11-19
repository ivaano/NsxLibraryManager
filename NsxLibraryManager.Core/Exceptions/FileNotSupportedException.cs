namespace NsxLibraryManager.Core.Exceptions;

public class FileNotSupportedException : Exception
{
    public FileNotSupportedException(string? filePath)
    {
        FilePath = filePath;
    }

    public string? FilePath { get; }
}