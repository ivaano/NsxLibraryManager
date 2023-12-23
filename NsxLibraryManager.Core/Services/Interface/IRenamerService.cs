﻿using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Core.Services.Interface;

public interface IRenamerService
{
    Task<string> BuildNewFileNameAsync(LibraryTitle fileInfo, string filePath);
    Task<string> CalculateSampleFileName(string templateText, PackageTitleType type, string inputFile, string basePath);
    Task<RenamerSettings> SaveRenamerSettingsAsync(RenamerSettings settings);
    Task<RenamerSettings> LoadRenamerSettingsAsync();
    Task<FluentValidation.Results.ValidationResult> ValidateRenamerSettingsAsync(RenamerSettings settings);
}