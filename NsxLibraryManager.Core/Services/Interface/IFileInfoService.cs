using Common.Services;
using NsxLibraryManager.Core.Models;

namespace NsxLibraryManager.Core.Services.Interface;

public interface IFileInfoService
{
    Task<Result<IEnumerable<string>>> GetFileNames(string filePath, bool recursive = false);

    Task<bool> IsDirectoryEmpty(string? directoryPath);
    IEnumerable<string> GetDirectoryFiles(string filePath);

    Task<IEnumerable<string>> GetRecursiveFiles(string filePath);

    Task<LibraryTitle> GetFileInfo(string filePath, bool detailed);
    
    Task<LibraryTitle> GetFileInfoFromFileName(string filePath);
    bool TryGetFileInfoFromFileName(string filePath, out LibraryTitle libraryTitle);

    Task<long?> GetFileSize(string filePath);
    Task<string?> GetFileIcon(string filePath);
}