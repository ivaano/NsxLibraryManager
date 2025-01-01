using System.Text.RegularExpressions;
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
using NsxLibraryManager.Services.Interface;

namespace NsxLibraryManager.Services;

public class SqlRenamerService(
    TitledbDbContext titledbDbContext, 
    IValidator<PackageRenamerSettings> validator,
    IValidator<BundleRenamerSettings> validatorBundle,
    IFileInfoService fileInfoService,
    ILogger<SqlRenamerService> logger)
    : ISqlRenamerService
{
    private readonly TitledbDbContext _titledbDbContext = titledbDbContext ?? throw new ArgumentNullException(nameof(titledbDbContext));
    private readonly IValidator<PackageRenamerSettings> _validator = validator;
    private readonly IValidator<BundleRenamerSettings> _validatorBundle = validatorBundle;
    private readonly IFileInfoService _fileInfoService = fileInfoService;
    private PackageRenamerSettings _packageRenamerSettings = null!;
    private BundleRenamerSettings _bundleRenamerSettings = null!;
    private readonly ILogger<SqlRenamerService> _logger = logger;
    private static char[] GetInvalidAdditionalChars() =>
    [
        '™', '©', '®', '~', '#', '%', '&', ':'
    ];
    
    private async Task<(string, bool, string)> TryBuildNewFileNameAsync(LibraryTitle fileInfo, string file)
    {
        try
        {
            var newPath = await BuildNewFileNameAsync(fileInfo, file);
            return (newPath, false, string.Empty);
        }
        catch (Exception e)
        {
            _logger.LogError("Error building new file name for {file} - {message}", file, e.Message);
            return (string.Empty, true, e.Message);
        }
    }

    private Task<(string, bool, string)> ValidateDestinationFileAsync(string file, string newPath)
    {
        if (string.IsNullOrEmpty(newPath))
        {
            _logger.LogError("Error building new file name for {file} - {message}", file, "New path is empty");
            return Task.FromResult((string.Empty, true, "New path is empty"));
        }

        if (file == newPath)
        {
            _logger.LogError("Error building new file name for {file} - {message}", file, "New path is the same as the old path");
            return Task.FromResult((string.Empty, true, "New path is the same as the old path"));
        }

        if (File.Exists(newPath))
        {
            _logger.LogError("Error building new file name for {file} - {message}", file, "File already exists");
            return Task.FromResult((string.Empty, true, "File already exists"));
        }
        var newDirName = Path.GetDirectoryName(newPath);
        if (!Path.Exists(newDirName) && newDirName is not null)
        { 
            Directory.CreateDirectory(newDirName);
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
    
    private Task<string> TemplateReplaceAsync(string renameTemplate, LibraryTitle fileInfo)
    {
        var newPath = renameTemplate;
        foreach (var (key, pattern) in RenamerTemplateFields.TemplateFieldMappings)
        {
            var match = Regex.Match(renameTemplate, pattern, RegexOptions.IgnoreCase);
            if (!match.Success) continue;
            var safeTitleName = string.Empty;
            var safeAppTitleName = string.Empty;
            if (fileInfo.TitleName is not null)
            {
                safeTitleName = RemoveIllegalCharacters(fileInfo.TitleName);    
            }
            if (fileInfo.ApplicationTitleName is not null)
            {
                safeAppTitleName = RemoveIllegalCharacters(fileInfo.ApplicationTitleName);
            }
            
            
            var replacement = key switch
            {
                TemplateField.BasePath  => _packageRenamerSettings.OutputBasePath,
                TemplateField.TitleName => safeTitleName,
                TemplateField.TitleId   => fileInfo.TitleId,
                TemplateField.Version   => fileInfo.TitleVersion.ToString(),
                TemplateField.Extension => fileInfo.PackageType.ToString().ToLower(),
                TemplateField.AppName   => safeAppTitleName,
                TemplateField.PatchId   => fileInfo.PatchTitleId,
                TemplateField.PatchNum  => fileInfo.PatchNumber.ToString(),
                _                       => string.Empty
            };
            
            if (replacement is null)
                throw new InvalidPathException($"No replacement for {key} value most likely due to missing titledb entry");
            
            newPath = TokenReplace(renameTemplate, pattern, replacement);
            renameTemplate = newPath;
        }
        return Task.FromResult(newPath);
    }
   
    
    public async Task<IEnumerable<RenameTitle>> GetFilesToRenameAsync(string inputPath, bool recursive = false)
    {
        var files = await _fileInfoService.GetFileNames(inputPath, recursive);
        var fileList = new List<RenameTitle>();
        
        // clean up this mess later
        foreach (var file in files)
        {
            var fileInfo = await _fileInfoService.GetFileInfo(file, false);
            
            if (fileInfo?.Error == true)
            {
                fileList.Add(new RenameTitle(file, string.Empty, string.Empty, string.Empty, false, fileInfo.Error, fileInfo.ErrorMessage));
                continue;
            }

            if (fileInfo is not null && string.IsNullOrEmpty(fileInfo.TitleName))
            {
                var titledbTitle = await _titledbDbContext.Titles.FirstOrDefaultAsync(t => t.ApplicationId == fileInfo.TitleId);
                if (titledbTitle is not null)
                {
                    fileInfo.TitleName = titledbTitle.TitleName;
                    if (fileInfo.ApplicationTitleId is not null && titledbTitle.ContentType != TitleContentType.Base)
                    {
                        var appTitledbTitle =
                            await _titledbDbContext.Titles.FirstOrDefaultAsync(t =>
                                t.ApplicationId == fileInfo.ApplicationTitleId);
                        if (appTitledbTitle != null)
                        {
                            fileInfo.ApplicationTitleName = appTitledbTitle.TitleName;
                        }
                    }
                }
            }

            if (fileInfo is null)
            {
                fileList.Add(new RenameTitle(file, string.Empty, string.Empty, string.Empty, false, true,
                    "Unable to get file info"));
                continue;
            }
            
            var (newPath, error, errorMessage) = await TryBuildNewFileNameAsync(fileInfo, file);
            if (error)
            {
                fileList.Add(new RenameTitle(file, string.Empty, string.Empty, string.Empty, false, error,
                    errorMessage));
                continue;
            }

            (newPath, error, errorMessage) = await ValidateDestinationFileAsync(file, newPath);
            if (error)
            {
                fileList.Add(new RenameTitle(file, string.Empty, string.Empty, string.Empty, false, error,
                    errorMessage));
                continue;
            }

            fileList.Add(new RenameTitle(file, newPath, fileInfo.TitleId, fileInfo.TitleName, false, error,
                errorMessage));
        }

        return fileList;
    }

    public async Task<string> BuildNewFileNameAsync(LibraryTitle fileInfo, string filePath)
    {
        fileInfo.FileName = filePath;

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
        
        return await TemplateReplaceAsync(renameTemplate, fileInfo);

    }

    public async Task<string> CalculateSampleFileName(string templateText, 
        PackageTitleType packageType, string inputFile, string basePath)
    {
        var libraryType = packageType switch
        {
            PackageTitleType.NspBase   => TitleLibraryType.Base,
            PackageTitleType.NspUpdate => TitleLibraryType.Update,
            PackageTitleType.NspDlc    => TitleLibraryType.DLC,
            PackageTitleType.BundleBase => TitleLibraryType.Base,
            PackageTitleType.BundleUpdate => TitleLibraryType.Update,
            PackageTitleType.BundleDlc    => TitleLibraryType.DLC,
            _                          => TitleLibraryType.Unknown
        };
        
        var fileInfo = new LibraryTitle
        {
            PackageType = AccuratePackageType.NSP,
            Type = libraryType,
            TitleName = "Some Title Name",
            TitleId = "0010000",
            FileName = inputFile,
            ApplicationTitleName = "Some App Name",
            PatchNumber = 1,
            PatchTitleId = "0000001",
        };
        return await TemplateReplaceAsync(templateText, fileInfo);
    }

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
                if (file.DestinationFileName is not null)
                {
                    File.Move(file.SourceFileName, file.DestinationFileName);
                    renamedFiles.Add(file);
                }
                else
                {
                    var renameTitle = file with { Error = true, ErrorMessage = "Empty destination file name" };
                    renamedFiles.Add(renameTitle);
                }
                

            }
            catch (Exception e)
            {
                _logger.LogError("Error renaming file {file} - {message}", file.SourceFileName, e.Message);
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

    public async Task<ValidationResult> ValidateRenamerSettingsAsync(PackageRenamerSettings settings)
    {
        var validationResult = await _validator.ValidateAsync(settings);
        return validationResult;
    }
    
    public async Task<ValidationResult> ValidateRenamerSettingsAsync(BundleRenamerSettings settings)
    {
        var validationResult = await _validatorBundle.ValidateAsync(settings);
        return validationResult;
    }
}