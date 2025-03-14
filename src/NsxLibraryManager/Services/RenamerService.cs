using System.Globalization;
using System.Text.RegularExpressions;
using Common.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using NsxLibraryManager.Core.Exceptions;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Data;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using NsxLibraryManager.Shared.Mapping;
using NsxLibraryManager.Shared.Settings;
using NsxLibraryManager.Utils;
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
    
    private async Task<(string, bool, string)> TryBuildNewFileNameAsync(LibraryTitleDto fileInfo, string file, RenameType renameType)
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

    private async Task<Result<bool>> UpdateLibraryTitleFileNameAsync(int id, string fileName)
    {
        var title = await _nsxLibraryDbContext.Titles.FirstOrDefaultAsync(x => x.Id == id);
        if (title is null) return Result.Failure<bool>("Title not found");
        title.FileName = fileName;
        await _nsxLibraryDbContext.SaveChangesAsync();
        return Result.Success(true);
    }
    public async Task<IEnumerable<RenameTitleDto>> RenameFilesAsync(IEnumerable<RenameTitleDto> filesToRename)
    {
        var renamedFiles = new List<RenameTitleDto>();
        foreach (var renameTitleDto in filesToRename)
        {
            if (renameTitleDto.Error || renameTitleDto.RenamedSuccessfully)
            {
                renamedFiles.Add(renameTitleDto);
                continue;
            }
            
            try
            {
                var newDirName = Path.GetDirectoryName(renameTitleDto.DestinationFileName);
                if (!Path.Exists(newDirName) && newDirName is not null)
                { 
                    Directory.CreateDirectory(newDirName);
                }
                
                if (renameTitleDto.DestinationFileName is not null)
                {
                    File.Move(renameTitleDto.SourceFileName, renameTitleDto.DestinationFileName);
                    if (renameTitleDto is { UpdateLibrary: true, Id: > 0 })
                    {
                        await UpdateLibraryTitleFileNameAsync(renameTitleDto.Id,
                            renameTitleDto.DestinationFileName);
                    }
                    renameTitleDto.RenamedSuccessfully = true;
                    renamedFiles.Add(renameTitleDto);
                }
                else
                {
                    renameTitleDto.Error = true;
                    renameTitleDto.ErrorMessage = "Empty destination file name";
                    renamedFiles.Add(renameTitleDto);
                }
            }
            catch (Exception e)
            {
                logger.LogError("Error renaming file {file} - {message}", renameTitleDto.SourceFileName, e.Message);
                renameTitleDto.RenamedSuccessfully = false;
                renameTitleDto.Error = true;
                renameTitleDto.ErrorMessage = e.Message;
                renamedFiles.Add(renameTitleDto);
            }
        }

        return renamedFiles.AsEnumerable();
    }

    public async Task<Result<string>> GetNewFileName(string renameTemplate, LibraryTitleDto libraryTitle, RenameType renameType)
    {
         return Result.Success(await TemplateReplaceAsync(renameTemplate, libraryTitle, renameType));
    }

    public Result<string> GetRenameTemplate(RenameType renameType, TitleContentType contentType, AccuratePackageType accuratePackageType)
    {
        string templateText;
        switch (renameType)
        {
            case RenameType.Bundle:
            {
                var renameTemplate = contentType switch
                {
                    TitleContentType.Base => _bundleRenamerSettings.BundleBase,
                    TitleContentType.Update => _bundleRenamerSettings.BundleUpdate,
                    TitleContentType.DLC => _bundleRenamerSettings.BundleDlc,
                    _ => string.Empty
                };
                templateText = $"{{BasePath}}{renameTemplate}";
                break;
            }
            case RenameType.Collection:
                var collectionRenameTemplate = contentType switch
                {
                    TitleContentType.Base => _collectionRenamerSettings.BundleBase,
                    TitleContentType.Update => _collectionRenamerSettings.BundleUpdate,
                    TitleContentType.DLC => _collectionRenamerSettings.BundleDlc,
                    _ => string.Empty
                };
                templateText = $"{{BasePath}}{collectionRenameTemplate}";
                break;
            case RenameType.PackageType:
            {
                templateText = accuratePackageType switch
                {
                    AccuratePackageType.NSP => contentType switch
                    {
                        TitleContentType.Base   => _packageRenamerSettings.NspBasePath,
                        TitleContentType.Update => _packageRenamerSettings.NspUpdatePath,
                        TitleContentType.DLC    => _packageRenamerSettings.NspDlcPath,
                        _                       => string.Empty
                    },
                    AccuratePackageType.NSZ => contentType switch
                    {
                        TitleContentType.Base   => _packageRenamerSettings.NszBasePath,
                        TitleContentType.Update => _packageRenamerSettings.NszUpdatePath,
                        TitleContentType.DLC    => _packageRenamerSettings.NszDlcPath,
                        _                       => string.Empty
                    },
                    AccuratePackageType.XCI => contentType switch
                    {
                        TitleContentType.Base   => _packageRenamerSettings.XciBasePath,
                        TitleContentType.Update => _packageRenamerSettings.XciUpdatePath,
                        TitleContentType.DLC    => _packageRenamerSettings.XciDlcPath,
                        _                       => string.Empty
                    },
                    AccuratePackageType.XCZ => contentType switch
                    {
                        TitleContentType.Base   => _packageRenamerSettings.XczBasePath,
                        TitleContentType.Update => _packageRenamerSettings.XczUpdatePath,
                        TitleContentType.DLC    => _packageRenamerSettings.XczDlcPath,
                        _                       => string.Empty
                    },
                    _ => throw new Exception("Unknown package type")
                };
                break;
            }
            default:
                return Result.Failure<string>("Unknown rename type");
        }
        
        return Result.Success(templateText);
    }


    public async Task<string> CalculateSampleFileName(string templateText, 
    TitlePackageType titlePackageType, string inputFile, RenameType renameType)
    {
        var fileInfo = new LibraryTitleDto
        {
            PackageType = AccuratePackageType.NSP,
            TitleName = "Title Name",
            ApplicationId = "0010000",
            FileName = inputFile,
            OtherApplicationName = "BaseApp Name",
            PatchNumber = 1,
            PatchTitleId = "0000001",
            Region = "US",
            UpdatesCount = 5,
            Collection = new CollectionDto() {Id = 1, Name = "Collection Name"},
            DlcCount = 3,
            Size = 1000000000,
            ContentType = (renameType, titlePackageType) switch
            {
                (RenameType.Collection, TitlePackageType.BundleBase)   => TitleContentType.Base,
                (RenameType.Collection, TitlePackageType.BundleUpdate) => TitleContentType.Update,
                (RenameType.Collection, TitlePackageType.BundleDlc)    => TitleContentType.DLC,
            
                (RenameType.Bundle, TitlePackageType.BundleBase)   => TitleContentType.Base,
                (RenameType.Bundle, TitlePackageType.BundleUpdate) => TitleContentType.Update,
                (RenameType.Bundle, TitlePackageType.BundleDlc)    => TitleContentType.DLC,
        
                (RenameType.PackageType, TitlePackageType.NspBase)   => TitleContentType.Base,
                (RenameType.PackageType, TitlePackageType.NspUpdate) => TitleContentType.Update,
                (RenameType.PackageType, TitlePackageType.NspDlc)    => TitleContentType.DLC,
                _ => TitleContentType.Unknown
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
        string renameTemplate, LibraryTitleDto fileInfo, RenameType renameType)
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
            
            if (!string.IsNullOrEmpty(fileInfo.OtherApplicationName))
            {
                if ((renameType == RenameType.Bundle && _bundleRenamerSettings.TitlesForceUppercase) ||
                    (renameType == RenameType.Collection && _collectionRenamerSettings.TitlesForceUppercase) ||
                    (renameType == RenameType.PackageType && _packageRenamerSettings.TitlesForceUppercase))
                {
                    fileInfo.OtherApplicationName = CustomTitleCase(fileInfo.OtherApplicationName);
                }
                safeAppTitleName = RemoveIllegalCharacters(fileInfo.OtherApplicationName);
            }

            if (fileInfo is { ContentType: TitleContentType.Update, OtherApplicationName: null })
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
                TemplateField.CollectionName => fileInfo.Collection?.Name,
                TemplateField.TitleName      => safeTitleName,
                TemplateField.TitleId        => fileInfo.ApplicationId,
                TemplateField.Version        => fileInfo.Version.ToString(),
                TemplateField.Extension      => fileInfo.PackageType.ToString().ToLower(),
                TemplateField.AppName        => safeAppTitleName,
                TemplateField.Region         =>  fileInfo.Region,
                TemplateField.PatchId        => fileInfo.PatchTitleId,
                TemplateField.PackageType    => fileInfo.PackageType.ToString().ToUpper(),
                TemplateField.Size           => fileInfo.Size.ToHumanReadableBytes(),
                TemplateField.PatchCount     => fileInfo.UpdatesCount.ToString(),
                TemplateField.DlcCount       => fileInfo.DlcCount.ToString(),
                _                            => string.Empty
            };
            
            if (replacement is null)
                throw new InvalidPathException($"No replacement for {key} value most likely due to missing titledb entry");
            
            newPath = TokenReplace(renameTemplate, pattern, replacement);
            renameTemplate = newPath;
        }
        return Task.FromResult(newPath);
    }

    private async Task<Result<LibraryTitleDto>> GetAggregatedFileInfo(string fileLocation, bool useEnglishNaming)
    {
        try
        {
            var fileInfoResult = await fileInfoService.GetFileInfo(fileLocation, false);
            if (fileInfoResult.IsFailure)
            {
                return Result.Failure<LibraryTitleDto>(fileInfoResult.Error!);
            }
            
            var fileInfo = fileInfoResult.Value;
            
            var titledbTitle =
                await _titledbDbContext.Titles.FirstOrDefaultAsync(t => t.ApplicationId == fileInfo.ApplicationId);
            if (titledbTitle is null)
            {
                //try to find OtherApplicationName in titledb
                if (fileInfo.ContentType is TitleContentType.Update or TitleContentType.DLC
                    && string.IsNullOrWhiteSpace(fileInfo.OtherApplicationName) && !string.IsNullOrEmpty(fileInfo.OtherApplicationId))
                {
                    var otherApplication = await _titledbDbContext.Titles.FirstOrDefaultAsync(t => t.OtherApplicationId == fileInfo.OtherApplicationId);
                    if (otherApplication is null) return Result.Success(fileInfo);
            
                    fileInfo.OtherApplicationName = otherApplication.TitleName;
                }
                return Result.Success(fileInfo);
            }
            //prefer Name  from titledb instead of the file
            fileInfo.TitleName = titledbTitle.TitleName;

            if (useEnglishNaming)
            {
                if (LanguageChecker.IsNonEnglish(fileInfo.TitleName))
                {
                    var applicationId = titledbTitle.ContentType switch
                    {
                        TitleContentType.Base => titledbTitle.ApplicationId,
                        TitleContentType.Update => titledbTitle.OtherApplicationId,
                        TitleContentType.DLC => titledbTitle.OtherApplicationId,
                        _ => titledbTitle.ApplicationId
                    };

                    var nswName = _titledbDbContext.NswReleaseTitles.FirstOrDefault(x => x.ApplicationId == applicationId);
                    if (nswName is not null)
                    {
                        if (titledbTitle.ContentType is TitleContentType.DLC)
                        {
                            var nswDlcName = _titledbDbContext.NswReleaseTitles.FirstOrDefault(x => x.ApplicationId == titledbTitle.ApplicationId);
                            if (nswDlcName is not null)
                            {
                                fileInfo.TitleName = nswDlcName.TitleName;
                            }
                        }
                        else
                        {
                            fileInfo.TitleName = nswName.TitleName;
                        }
                        fileInfo.Publisher = nswName.Publisher;
                        fileInfo.OtherApplicationName = nswName.TitleName;
                    }
                }
            }


            fileInfo.UpdatesCount = titledbTitle.UpdatesCount;
            fileInfo.DlcCount = titledbTitle.DlcCount;

            if (titledbTitle.ContentType == TitleContentType.Base
                || string.IsNullOrEmpty(titledbTitle.OtherApplicationId)) return Result.Success(fileInfo);

            if (fileInfo.OtherApplicationName is not null) return Result.Success(fileInfo);
            
            var parentTitle = await _titledbDbContext.Titles.FirstOrDefaultAsync(t => t.ApplicationId == titledbTitle.OtherApplicationId);
            fileInfo.OtherApplicationName = parentTitle?.TitleName;

            return Result.Success(fileInfo);
        }
        catch (Exception e)
        {
            return Result.Failure<LibraryTitleDto>(e.Message);
        }
    }

    public async Task<IEnumerable<RenameTitleDto>> GetFilesToRenameAsync(
        string inputPath, RenameType renameType, bool recursive = false)
    {
        var filesResult = await fileInfoService.GetFileNames(inputPath, recursive);
        if (filesResult.IsFailure)
        {
            throw new InvalidPathException(filesResult.Error ?? inputPath);
        }

        var files = filesResult.Value;
        var useEnglishNaming = renameType switch
        {
            RenameType.PackageType => _packageRenamerSettings.UseEnglishNaming,
            RenameType.Bundle => _bundleRenamerSettings.UseEnglishNaming,
            RenameType.Collection => _collectionRenamerSettings.UseEnglishNaming,
            _ => false
        };

        var fileList = new List<RenameTitleDto>();
        foreach (var file in files)
        {
            logger.LogInformation("Analyzing {}", file);
            var fileInfoResult = await GetAggregatedFileInfo(file, useEnglishNaming);
            if (fileInfoResult.IsFailure)
            {
                fileList.Add(new RenameTitleDto
                {
                    SourceFileName = file,
                    Error = true,
                    ErrorMessage = fileInfoResult.Error
                });
                continue;
            }

            var fileInfo = fileInfoResult.Value;
            var (newPath, error, errorMessage) = await TryBuildNewFileNameAsync(fileInfo, file, renameType);
            if (error)
            {
                fileList.Add(new RenameTitleDto
                {
                    SourceFileName = file,
                    Error = true,
                    ErrorMessage = errorMessage
                });
                continue;
            }

            (newPath, error, errorMessage) = await ValidateDestinationFileAsync(file, newPath);
            if (error)
            {
                fileList.Add(new RenameTitleDto
                {
                    SourceFileName = file,
                    Error = true,
                    TitleName = fileInfo.TitleName,
                    TitleId = fileInfo.ApplicationId,
                    ErrorMessage = errorMessage
                });
                continue;
            }

            fileList.Add(new RenameTitleDto
            {
                SourceFileName = file,
                DestinationFileName = newPath,
                TitleName = fileInfo.TitleName,
                TitleId = fileInfo.ApplicationId,
            });

        }

        return fileList;
    }

    private async Task<string> BuildNewFileNameAsync(
        LibraryTitleDto fileInfo, 
        string filePath, 
        RenameType renameType)
    {
        fileInfo.FileName = filePath;
        
        var templateResult = GetRenameTemplate(renameType, fileInfo.ContentType, fileInfo.PackageType);
        if (templateResult.IsFailure)
        {
            logger.LogError(templateResult.Error);
        }
        
        return await TemplateReplaceAsync(templateResult.Value, fileInfo, renameType);
        
    }
}