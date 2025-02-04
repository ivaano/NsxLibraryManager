using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using Common.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Exceptions;
using NsxLibraryManager.Core.Mapping;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Core.Settings;
using NsxLibraryManager.Data;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Services.Interface;
using IRenamerService = NsxLibraryManager.Services.Interface.IRenamerService;

namespace NsxLibraryManager.Services;

public class RenamerService(
    TitledbDbContext titledbDbContext,
    NsxLibraryDbContext nsxLibraryDbContext,
    IValidator<PackageRenamerSettings> validatorPackage,
    IValidator<BundleRenamerSettings> validatorBundle,
    IValidator<CollectionRenamerSettings> validatorCollection,
    IFileInfoService fileInfoService,
    ILogger<RenamerService> logger)
    : IRenamerService
{
    private readonly TitledbDbContext _titledbDbContext = titledbDbContext ?? throw new ArgumentNullException(nameof(titledbDbContext));
    private readonly NsxLibraryDbContext _nsxLibraryDbContext = nsxLibraryDbContext ?? throw new ArgumentNullException(nameof(nsxLibraryDbContext));
    private PackageRenamerSettings _packageRenamerSettings = null!;
    private BundleRenamerSettings _bundleRenamerSettings = null!;
    private CollectionRenamerSettings _collectionRenamerSettings = null!;
    

    private static char[] GetInvalidAdditionalChars() =>
    [
        '™', '©', '®', '~', '#', '%', '&', ':',
        '\'', '"', '"', '–', '—', '…', 
        '′', '″', '‴', 
        '¢', '¥', '€', '£', 
        '±', '×', '÷', '≠', '≤', '≥', 
        '¡', '¿', 
        '\u200B', '\u200C', '\u200D', '\uFEFF' 
    ];

    #region RenamerSettings
    public Task<PackageRenamerSettings> LoadRenamerSettingsAsync(PackageRenamerSettings settings)
    {
        _packageRenamerSettings = settings;
        return Task.FromResult(_packageRenamerSettings);
    }

    public Task<BundleRenamerSettings> LoadRenamerSettingsAsync(BundleRenamerSettings settings)
    {
        _bundleRenamerSettings = settings;
        return Task.FromResult(_bundleRenamerSettings);
    }

    public Task<CollectionRenamerSettings> LoadRenamerSettingsAsync(CollectionRenamerSettings settings)
    {
        _collectionRenamerSettings = settings;
        return Task.FromResult(_collectionRenamerSettings);
    }
    
    private async Task<(string, bool, string)> TryBuildNewFileNameAsync(LibraryTitle fileInfo, string file, RenameType renameType)
    {
        try
        {
            var newPath = await BuildNewFileNameAsync(fileInfo, file, renameType);
            return (newPath, false, string.Empty);
        }
        catch (Exception e)
        {
            logger.LogError("Error building new file name for {file} - {message}", file, e.Message);
            return (string.Empty, true, e.Message);
        }
    }

    public async Task<ValidationResult> ValidateRenamerSettingsAsync(PackageRenamerSettings settings)
    {
        var validationResult = await validatorPackage.ValidateAsync(settings);
        return validationResult;
    }
    
    public async Task<ValidationResult> ValidateRenamerSettingsAsync(BundleRenamerSettings settings)
    {
        var validationResult = await validatorBundle.ValidateAsync(settings);
        return validationResult;
    }
    
    public async Task<ValidationResult> ValidateRenamerSettingsAsync(CollectionRenamerSettings settings)
    {
        var validationResult = await validatorCollection.ValidateAsync(settings);
        return validationResult;
    }
    #endregion
    
    public Task<IEnumerable<RenameTitle>> RenameFilesAsync(IEnumerable<RenameTitle> filesToRename)
    {
        var renamedFiles = new List<RenameTitle>();
        foreach (var file in filesToRename)
        {
            if (file.Error || file.RenamedSuccessfully)
            {
                renamedFiles.Add(file);
                continue;
            }
            
            try
            {
                var newDirName = Path.GetDirectoryName(file.DestinationFileName);
                if (!Path.Exists(newDirName) && newDirName is not null)
                { 
                    Directory.CreateDirectory(newDirName);
                }
                
                if (file.DestinationFileName is not null)
                {
                    File.Move(file.SourceFileName, file.DestinationFileName);
                    renamedFiles.Add(file with { RenamedSuccessfully = true });
                }
                else
                {
                    var renameTitle = file with { Error = true, ErrorMessage = "Empty destination file name" };
                    renamedFiles.Add(renameTitle);
                }
            }
            catch (Exception e)
            {
                logger.LogError("Error renaming file {file} - {message}", file.SourceFileName, e.Message);
                renamedFiles.Add(file with
                {
                    RenamedSuccessfully = false,
                    Error = true,
                    ErrorMessage = e.Message
                });
            }
        }

        return Task.FromResult(renamedFiles.AsEnumerable());
    }
    
    public async Task<string> CalculateSampleFileName(string templateText, 
    TitlePackageType titlePackageType, string inputFile, RenameType renameType)
    {
        var fileInfo = new LibraryTitle
        {
            PackageType = AccuratePackageType.NSP,
            TitleName = "Title Name",
            TitleId = "0010000",
            FileName = inputFile,
            ApplicationTitleName = "BaseApp Name",
            PatchNumber = 1,
            PatchTitleId = "0000001",
            Region = "US",
            UpdatesCount = 5,
            Collection = "Collection",
            DlcCount = 3,
            Size = 1000000000,
            Type = (renameType, titlePackageType) switch
            {
                (RenameType.Collection, TitlePackageType.BundleBase)   => TitleLibraryType.Base,
                (RenameType.Collection, TitlePackageType.BundleUpdate) => TitleLibraryType.Update,
                (RenameType.Collection, TitlePackageType.BundleDlc)    => TitleLibraryType.DLC,
            
                (RenameType.Bundle, TitlePackageType.BundleBase)   => TitleLibraryType.Base,
                (RenameType.Bundle, TitlePackageType.BundleUpdate) => TitleLibraryType.Update,
                (RenameType.Bundle, TitlePackageType.BundleDlc)    => TitleLibraryType.DLC,
        
                (RenameType.PackageType, TitlePackageType.NspBase)   => TitleLibraryType.Base,
                (RenameType.PackageType, TitlePackageType.NspUpdate) => TitleLibraryType.Update,
                (RenameType.PackageType, TitlePackageType.NspDlc)    => TitleLibraryType.DLC,
                _ => TitleLibraryType.Unknown
            }
        };
        
        return await TemplateReplaceAsync(templateText, fileInfo, renameType);
    }

    public async Task<bool> DeleteEmptyFoldersAsync(string sourceFolder)
    {
        var inputDirectories = Directory.EnumerateDirectories(sourceFolder, "*", SearchOption.AllDirectories);
        var result = true;
        foreach (var directoryName in inputDirectories)
        {
            var isEmpty = await fileInfoService.IsDirectoryEmpty(directoryName);

            if (!isEmpty) continue;
            try
            {
                Directory.Delete(directoryName, true);
            }
            catch (Exception)
            {
                result = false;
            }
        }
        return result;
    }
    
    
    private Task<(string, bool, string)> ValidateDestinationFileAsync(string file, string newPath)
    {
        if (string.IsNullOrEmpty(newPath))
        {
            logger.LogError("Error building new file name for {file} - {message}", file, "New path is empty");
            return Task.FromResult((string.Empty, true, "New path is empty"));
        }

        if (file == newPath)
        {
            logger.LogError("Error building new file name for {file} - {message}", file, "New path is the same as the old path");
            return Task.FromResult((string.Empty, true, "New path is the same as the old path"));
        }

        if (File.Exists(newPath))
        {
            logger.LogError("Error building new file name for {file} - {message}", file, "File already exists");
            return Task.FromResult((newPath, true, "File already exists"));
        }

        return Task.FromResult((newPath, false, string.Empty));
    }
    
    private static string TokenReplace(string input, string pattern, string replacement)
    {
        return Regex.Replace(input, pattern, replacement, RegexOptions.IgnoreCase);
    }
    
    private static string RemoveIllegalCharacters(string input)
    {
        var invalidChars = Path.GetInvalidFileNameChars()
            .Union(Path.GetInvalidPathChars())
            .Union(GetInvalidAdditionalChars()).ToArray();

        return invalidChars.Aggregate(input, (current, c) => current.Replace(c.ToString(), ""));
    }
    
    private static string CustomTitleCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        const string romanPattern = @"\b(IX|IV|V?I{1,3}|X{1,3})\b";
        const string ordinalPattern = @"\b\d+(st|nd|rd|th)\b";

        var specialTerms  = new List<string>();
        
        
        var markedText = Regex.Replace(
            input,
            ordinalPattern,
            match => {
                var number = Regex.Match(match.Value, @"\d+").Value;
                var suffix = Regex.Match(match.Value, @"(st|nd|rd|th)").Value.ToLower();
                specialTerms.Add(number + suffix);
                return $"§§{specialTerms.Count - 1}§§";
            },
            RegexOptions.IgnoreCase
        );
        
        markedText = Regex.Replace(
            markedText,
            romanPattern,
            match => {
                specialTerms.Add(match.Value.ToUpper());
                return $"§§{specialTerms.Count - 1}§§";
            },
            RegexOptions.IgnoreCase
        );

        var textInfo = new CultureInfo("en-US", false).TextInfo;
        var titleCased = textInfo.ToTitleCase(markedText.ToLower());

        var result = Regex.Replace(titleCased, @"§§(\d+)§§", match =>
        {
            var index = int.Parse(match.Groups[1].Value);
            return specialTerms [index];
        });

        return result;
    }
    
    private Task<string> TemplateReplaceAsync(
        string renameTemplate, LibraryTitle fileInfo, RenameType renameType)
    {
        var newPath = renameTemplate;
        foreach (var (key, pattern) in RenamerTemplateFields.TemplateFieldMappings)
        {
            var match = Regex.Match(renameTemplate, pattern, RegexOptions.IgnoreCase);
            if (!match.Success) continue;
            var safeTitleName = string.Empty;
            var safeAppTitleName = string.Empty;
            
            if (!string.IsNullOrEmpty(fileInfo.TitleName))
            {
                if ((renameType == RenameType.Bundle && _bundleRenamerSettings.TitlesForceUppercase) ||
                    (renameType == RenameType.Collection && _collectionRenamerSettings.TitlesForceUppercase) ||
                    (renameType == RenameType.PackageType && _packageRenamerSettings.TitlesForceUppercase))
                {
                    fileInfo.TitleName = CustomTitleCase(fileInfo.TitleName);
                }
                safeTitleName = RemoveIllegalCharacters(fileInfo.TitleName);    
            }
            else
            {
                safeTitleName = (renameType == RenameType.Bundle) ? 
                    _bundleRenamerSettings.UnknownPlaceholder : _packageRenamerSettings.UnknownPlaceholder;
            }
            
            if (!string.IsNullOrEmpty(fileInfo.ApplicationTitleName))
            {
                if ((renameType == RenameType.Bundle && _bundleRenamerSettings.TitlesForceUppercase) ||
                    (renameType == RenameType.Collection && _collectionRenamerSettings.TitlesForceUppercase) ||
                    (renameType == RenameType.PackageType && _packageRenamerSettings.TitlesForceUppercase))
                {
                    fileInfo.ApplicationTitleName = CustomTitleCase(fileInfo.ApplicationTitleName);
                }
                safeAppTitleName = RemoveIllegalCharacters(fileInfo.ApplicationTitleName);
            }

            if (fileInfo is { Type: TitleLibraryType.Update, ApplicationTitleName: null })
            {
                safeAppTitleName = safeTitleName;
            }
            
            var replacement = key switch
            {
                TemplateField.BasePath => renameType switch
                {
                    RenameType.PackageType => _packageRenamerSettings.OutputBasePath,
                    RenameType.Bundle      => _bundleRenamerSettings.OutputBasePath,
                    RenameType.Collection  => _collectionRenamerSettings.OutputBasePath,
                    _                      => string.Empty
                },
                TemplateField.CollectionName => fileInfo.Collection,
                TemplateField.TitleName => safeTitleName,
                TemplateField.TitleId   => fileInfo.TitleId,
                TemplateField.Version   => fileInfo.TitleVersion.ToString(),
                TemplateField.Extension => fileInfo.PackageType.ToString().ToLower(),
                TemplateField.AppName   => safeAppTitleName,
                TemplateField.Region    =>  fileInfo.Region,
                TemplateField.PatchId   => fileInfo.PatchTitleId,
                TemplateField.Size      => fileInfo.Size.HasValue ? fileInfo.Size.Value.ToHumanReadableBytes() : string.Empty,
                TemplateField.PatchCount   => fileInfo.UpdatesCount.ToString(),
                TemplateField.DlcCount   => fileInfo.DlcCount.ToString(),
                _                       => string.Empty
            };
            
            if (replacement is null)
                throw new InvalidPathException($"No replacement for {key} value most likely due to missing titledb entry");
            
            newPath = TokenReplace(renameTemplate, pattern, replacement);
            renameTemplate = newPath;
        }
        return Task.FromResult(newPath);
    }

    private async Task<LibraryTitle?> GetAggregatedFileInfo(string fileLocation)
    {
        try
        {
            var fileInfo = await fileInfoService.GetFileInfo(fileLocation, false);
            var titledbTitle =
                await _titledbDbContext.Titles.FirstOrDefaultAsync(t => t.ApplicationId == fileInfo.TitleId);
            if (titledbTitle is null)
            {
                //try to find OtherApplicationName in titledb
                if (fileInfo.Type is TitleLibraryType.Update or TitleLibraryType.DLC
                    && string.IsNullOrWhiteSpace(fileInfo.ApplicationTitleName) && !string.IsNullOrWhiteSpace(fileInfo.ApplicationTitleId))
                {
                    var otherApplication = await _titledbDbContext.Titles.FirstOrDefaultAsync(t => t.OtherApplicationId == fileInfo.ApplicationTitleId);
                    if (otherApplication is null) return fileInfo;
            
                    fileInfo.ApplicationTitleName = otherApplication.TitleName;
                }
                return fileInfo;
            }

            //prefer Name  from titledb instead of the file
            fileInfo.TitleName = titledbTitle.TitleName;
            fileInfo.UpdatesCount = titledbTitle.UpdatesCount;
            fileInfo.DlcCount = titledbTitle.DlcCount;

            if (titledbTitle.ContentType == TitleContentType.Base
                || string.IsNullOrEmpty(titledbTitle.OtherApplicationId)) return fileInfo;

            var parentTitle = await _titledbDbContext.Titles.FirstOrDefaultAsync(t => t.ApplicationId == titledbTitle.OtherApplicationId);
            fileInfo.ApplicationTitleName = parentTitle?.TitleName;

            return fileInfo;
        }
        catch (Exception e)
        {
            return new LibraryTitle
            {
                Error = true,
                ErrorMessage = e.Message,
                TitleId = string.Empty,
                FileName = string.Empty,
            };
        }
    }
/*    
    private async Task<Result<bool>> BuildNewFileName(LibraryTitleDto libraryTitleDto, RenameType renameType)
    {
        var libraryTitle = new LibraryTitle
        {
            PackageType = libraryTitleDto.PackageType,
            TitleName = libraryTitleDto.TitleName,
            TitleId = libraryTitleDto.ApplicationId,
            FileName = libraryTitleDto.FileName ?? string.Empty,
            ApplicationTitleName = string.Empty,
            PatchNumber = 0,
            PatchTitleId = "0000001",
            Region = libraryTitleDto.Region,
            UpdatesCount = libraryTitleDto.UpdatesCount,
            Collection = libraryTitleDto.Collection.Name,
            DlcCount = libraryTitleDto.DlcCount,
            Size = libraryTitleDto.Size,
            Type = libraryTitleDto.ContentType switch
            {
                TitleContentType.Base => TitleLibraryType.Base,
                TitleContentType.Update => TitleLibraryType.Update,
                TitleContentType.DLC => TitleLibraryType.DLC,
                _ => TitleLibraryType.Unknown
            },

        };
        var prependBasePath = $"{{BasePath}}";
        var algo = await TemplateReplaceAsync(prependBasePath, libraryTitle, renameType);

        return false;
    }
*/

    
    public async Task<IEnumerable<RenameTitle>> GetFilesToRenameAsync(
        string inputPath, RenameType renameType, bool recursive = false)
    {
        var filesResult = await fileInfoService.GetFileNames(inputPath, recursive);
        if (filesResult.IsFailure)
        {
            throw new InvalidPathException(filesResult.Error ?? inputPath);
        }

        var files = filesResult.Value;
        
        var fileList = new List<RenameTitle>();
        foreach (var file in files)
        {
            logger.LogInformation("Analyzing {}", file);
            var fileInfo = await GetAggregatedFileInfo(file);

            if (fileInfo is null)
            {
                fileList.Add(new RenameTitle(file, string.Empty, string.Empty, string.Empty, false, true,
                    "Unable to get file info"));
                continue;
            }
            
            if (fileInfo?.Error == true)
            {
                fileList.Add(new RenameTitle(file, string.Empty, string.Empty, string.Empty, false, fileInfo.Error, fileInfo.ErrorMessage));
                continue;
            }
            
            var (newPath, error, errorMessage) = await TryBuildNewFileNameAsync(fileInfo, file, renameType);
            if (error)
            {
                fileList.Add(new RenameTitle(file, string.Empty, string.Empty, string.Empty, false, error,
                    errorMessage));
                continue;
            }

            (newPath, error, errorMessage) = await ValidateDestinationFileAsync(file, newPath);
            if (error)
            {
                fileList.Add(new RenameTitle(file, newPath, fileInfo.TitleId, fileInfo.TitleName, false, error,
                    errorMessage));
                continue;
            }

            fileList.Add(new RenameTitle(file, newPath, fileInfo.TitleId, fileInfo.TitleName, false, error,
                errorMessage));
        }

        return fileList;
    }

    private async Task<string> BuildNewFileNameAsync(
        LibraryTitle fileInfo, 
        string filePath, 
        RenameType renameType)
    {
        fileInfo.FileName = filePath;

        switch (renameType)
        {
            case RenameType.Bundle:
            {
                var renameTemplate = fileInfo.Type switch
                {
                    TitleLibraryType.Base => _bundleRenamerSettings.BundleBase,
                    TitleLibraryType.Update => _bundleRenamerSettings.BundleUpdate,
                    TitleLibraryType.DLC => _bundleRenamerSettings.BundleDlc,
                    _ => string.Empty
                };
                var prependBasePath = $"{{BasePath}}{renameTemplate}";
                return await TemplateReplaceAsync(prependBasePath, fileInfo, renameType);
            }
            case RenameType.PackageType:
            {
                // No demo support for now
                var renameTemplate = fileInfo.PackageType switch
                {
                    AccuratePackageType.NSP => fileInfo.Type switch
                    {
                        TitleLibraryType.Base   => _packageRenamerSettings.NspBasePath,
                        TitleLibraryType.Update => _packageRenamerSettings.NspUpdatePath,
                        TitleLibraryType.DLC    => _packageRenamerSettings.NspDlcPath,
                        _                       => string.Empty
                    },
                    AccuratePackageType.NSZ => fileInfo.Type switch
                    {
                        TitleLibraryType.Base   => _packageRenamerSettings.NszBasePath,
                        TitleLibraryType.Update => _packageRenamerSettings.NszUpdatePath,
                        TitleLibraryType.DLC    => _packageRenamerSettings.NszDlcPath,
                        _                       => string.Empty
                    },
                    AccuratePackageType.XCI => fileInfo.Type switch
                    {
                        TitleLibraryType.Base   => _packageRenamerSettings.XciBasePath,
                        TitleLibraryType.Update => _packageRenamerSettings.XciUpdatePath,
                        TitleLibraryType.DLC    => _packageRenamerSettings.XciDlcPath,
                        _                       => string.Empty
                    },
                    AccuratePackageType.XCZ => fileInfo.Type switch
                    {
                        TitleLibraryType.Base   => _packageRenamerSettings.XczBasePath,
                        TitleLibraryType.Update => _packageRenamerSettings.XczUpdatePath,
                        TitleLibraryType.DLC    => _packageRenamerSettings.XczDlcPath,
                        _                       => string.Empty
                    },
                    _ => throw new Exception("Unknown package type")
                };
                return await TemplateReplaceAsync(renameTemplate, fileInfo, renameType);
            }
            case RenameType.Collection:
            default:
                return string.Empty;
        }
    }
}