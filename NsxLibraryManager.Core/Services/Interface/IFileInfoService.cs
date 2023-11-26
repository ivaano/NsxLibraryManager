using NsxLibraryManager.Core.Models;

namespace NsxLibraryManager.Core.Services.Interface;

public interface IFileInfoService
{
    Task<IEnumerable<string>> GetFileNames(string filePath, bool recursive = false);

    IEnumerable<string> GetDirectoryFiles(string filePath);

    Task<IEnumerable<string>> GetRecursiveFiles(string filePath);

    Task<LibraryTitle> GetFileInfo(string filePath);
}