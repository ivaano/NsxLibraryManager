using NsxLibraryManager.Core.Models;

namespace NsxLibraryManager.Core.Services.Interface;

public interface IFileInfoService
{
    Task<IEnumerable<string>> GetFileNames(string filePath, bool recursive = false);

    IEnumerable<string> GetDirectoryFiles(string filePath);

    Task<IEnumerable<string>> GetRecursiveFiles(string filePath);

    Task<LibraryTitle> GetFileInfo(string filePath, bool detailed);
    
    Task<LibraryTitle> GetFileInfoFromFileName(string filePath);

    Task<long?> GetFileSize(string filePath);
    Task<string?> GetFileIcon(string filePath);
}