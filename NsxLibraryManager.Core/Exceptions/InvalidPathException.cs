namespace NsxLibraryManager.Core.Exceptions;

public class InvalidPathException : Exception
{
    public InvalidPathException(string invalidPath) : base(message: $"Invalid path: {invalidPath}")
    {
    }
}