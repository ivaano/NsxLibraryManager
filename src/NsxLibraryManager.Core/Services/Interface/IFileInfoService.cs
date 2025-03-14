using Common.Services;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Shared.Dto;

namespace NsxLibraryManager.Core.Services.Interface;

public interface IFileInfoService
{
    Task<Result<IEnumerable<string>>> GetFileNames(string filePath, bool recursive = false);

    Task<bool> IsDirectoryEmpty(string? directoryPath);
    IEnumerable<string> GetDirectoryFiles(string filePath);

    Task<IEnumerable<string>> GetRecursiveFiles(string filePath);

    Task<Result<LibraryTitleDto>> GetFileInfo(string filePath, bool detailed);
    
    Task<LibraryTitleDto> GetFileInfoFromFileName(string filePath);
    bool TryGetFileInfoFromFileName(string filePath, out LibraryTitleDto libraryTitle);

    Task<long> GetFileSize(string filePath);
    Task<string?> GetFileIcon(string filePath);
    Task<Result<bool>> DeleteIconDirectoryFiles();
}