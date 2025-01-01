using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Services.Interface;

public interface ISqlRenamerService
{
    Task<IEnumerable<RenameTitle>> GetFilesToRenameAsync(string filePath, bool recursive = false);
    Task<string> BuildNewFileNameAsync(LibraryTitle fileInfo, string filePath);
    Task<string> CalculateSampleFileName(string templateText, PackageTitleType type, string inputFile, string basePath);
    Task<PackageRenamerSettings> LoadRenamerSettingsAsync(PackageRenamerSettings settings);
    Task<BundleRenamerSettings> LoadRenamerSettingsAsync(BundleRenamerSettings settings);
    Task<IEnumerable<RenameTitle>> RenameFilesAsync(IEnumerable<RenameTitle> filesToRename);
    Task<FluentValidation.Results.ValidationResult> ValidateRenamerSettingsAsync(PackageRenamerSettings settings);
    Task<FluentValidation.Results.ValidationResult> ValidateRenamerSettingsAsync(BundleRenamerSettings settings);
}