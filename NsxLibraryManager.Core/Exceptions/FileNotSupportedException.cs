namespace NsxLibraryManager.Core.Exceptions;

[Serializable]
public class FileNotSupportedException : Exception
{
    public FileNotSupportedException()
    { }
    public FileNotSupportedException(string message) : base(message) { }
    public FileNotSupportedException(string message, string filePath) : base(message) { }
}