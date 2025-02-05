using Common.Services;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Shared.Enums;
using NsxLibraryManager.Shared.Settings;

namespace NsxLibraryManager.Services.Interface;

public interface IRenamerService
{
    Task<IEnumerable<RenameTitle>> GetFilesToRenameAsync(string filePath, RenameType renameType, bool recursive = false);
    Task<string> CalculateSampleFileName(string templateText, TitlePackageType type, string inputFile, RenameType renameType);
    Task<PackageRenamerSettings> LoadRenamerSettingsAsync(PackageRenamerSettings settings);
    Task<BundleRenamerSettings> LoadRenamerSettingsAsync(BundleRenamerSettings settings);
    Task<CollectionRenamerSettings> LoadRenamerSettingsAsync(CollectionRenamerSettings settings);
    Task<IEnumerable<RenameTitle>> RenameFilesAsync(IEnumerable<RenameTitle> filesToRename);
    Result<string> GetRenameTemplate(RenameType renameType, TitleContentType contentType, AccuratePackageType accuratePackageType);
    Task<bool> DeleteEmptyFoldersAsync(string sourceFolder);
    Task<FluentValidation.Results.ValidationResult> ValidateRenamerSettingsAsync(PackageRenamerSettings settings);
    Task<FluentValidation.Results.ValidationResult> ValidateRenamerSettingsAsync(BundleRenamerSettings settings);
    Task<FluentValidation.Results.ValidationResult> ValidateRenamerSettingsAsync(CollectionRenamerSettings settings);
}