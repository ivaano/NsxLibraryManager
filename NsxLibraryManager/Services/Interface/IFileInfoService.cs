using NsxLibraryManager.Models;

namespace NsxLibraryManager.Services;

public interface IFileInfoService
{
    Task<IEnumerable<string>> GetFileNames(string filePath, bool recursive = false);

    IEnumerable<string> GetDirectoryFiles(string filePath);

    Task<IEnumerable<string>> GetRecursiveFiles(string filePath);

    Task<LibraryTitle> GetFileInfo(string filePath);
}