using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Core.Services.Interface;

public interface IRenamerService
{
    Task<IEnumerable<RenameTitle>> GetFilesToRenameAsync(string filePath, bool recursive = false);
    Task<string> BuildNewFileNameAsync(LibraryTitle fileInfo, string filePath);
    Task<string> CalculateSampleFileName(string templateText, PackageTitleType type, string inputFile, string basePath);
    Task<RenamerSettings> SaveRenamerSettingsAsync(RenamerSettings settings);
    Task<RenamerSettings> LoadRenamerSettingsAsync();
    Task<IEnumerable<RenameTitle>> RenameFilesAsync(IEnumerable<RenameTitle> filesToRename);
    Task<FluentValidation.Results.ValidationResult> ValidateRenamerSettingsAsync(RenamerSettings settings);
}