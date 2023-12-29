﻿using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Exceptions;
using NsxLibraryManager.Core.Mapping;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Core.Services;

public class RenamerService : IRenamerService
{
    private readonly ILogger<RenamerService> _logger;
    private RenamerSettings _settings = default!;
    private readonly IDataService _dataService;
    private readonly IValidator<RenamerSettings> _validator;
    private readonly IFileInfoService _fileInfoService;
    private readonly ITitleDbService _titleDbService;

    public RenamerService(ILogger<RenamerService> logger,
        IDataService dataService,
        IValidator<RenamerSettings> validator,
        IFileInfoService fileInfoService, ITitleDbService titleDbService)
    {
        _logger = logger;
        _dataService = dataService;
        _validator = validator;
        _fileInfoService = fileInfoService;
        _titleDbService = titleDbService;
    }

    private Task<string> TemplateReplaceAsync(string renameTemplate, LibraryTitle fileInfo)
    {
        var newPath = renameTemplate;
        foreach (var (key, pattern) in RenamerTemplateFields.TemplateFieldMappings)
        {
            var match = Regex.Match(renameTemplate, pattern, RegexOptions.IgnoreCase);
            if (!match.Success) continue;

            var replacement = key switch
            {
                TemplateField.BasePath  => _settings.OutputBasePath,
                TemplateField.TitleName => fileInfo.TitleName,
                TemplateField.TitleId   => fileInfo.TitleId,
                TemplateField.Version   => fileInfo.TitleVersion.ToString(),
                TemplateField.Extension => fileInfo.PackageType.ToString().ToLower(),
                TemplateField.AppName   => fileInfo.ApplicationTitleName,
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
                var titledbTitle = await _titleDbService.GetTitle(fileInfo.TitleId);
                if (titledbTitle != null)
                {
                    fileInfo.TitleName = titledbTitle.Name;
                    if (fileInfo.ApplicationTitleId != null && titledbTitle.Type != TitleLibraryType.Base)
                    {
                        var appTitledbTitle = await _titleDbService.GetTitle(fileInfo.ApplicationTitleId);
                        if (appTitledbTitle != null)
                        {
                            fileInfo.ApplicationTitleName = appTitledbTitle.Name;
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
        
        var invalidFileNameChars = Path.GetInvalidFileNameChars();
        var invalidCharReplacement = string.Empty;
        var newFileName = Path.GetFileName(newPath);
        newFileName = invalidFileNameChars.Aggregate(newFileName, (current, invalidFileNameChar) => current.Replace(invalidFileNameChar.ToString(), invalidCharReplacement));

        var invalidDirNameChars = Path.GetInvalidPathChars();
        var invalidDirCharReplacement = string.Empty;
        var newDirName =Path.GetDirectoryName(newPath);
        newDirName = invalidDirNameChars.Aggregate(newDirName, (current, invalidDirNameChar) => current?.Replace(invalidDirNameChar.ToString(), invalidDirCharReplacement));

        if (newDirName is null)
        {
            _logger.LogError("Error building new file name for {file} - {message}", file, "New path is empty");
            return Task.FromResult((string.Empty, true, "New path is empty"));
        }
        
        newPath = Path.Combine(newDirName, newFileName);
        

        return Task.FromResult((newPath, false, string.Empty));
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
    
    public async Task<string> BuildNewFileNameAsync(LibraryTitle fileInfo, string filePath)
    {
        fileInfo.FileName = filePath;

        // No demo support for now
        var renameTemplate = fileInfo.PackageType switch
        {
            AccuratePackageType.NSP => fileInfo.Type switch
            {
                TitleLibraryType.Base   => _settings.NspBasePath,
                TitleLibraryType.Update => _settings.NspUpdatePath,
                TitleLibraryType.DLC    => _settings.NspDlcPath,
                _                       => string.Empty
            },
            AccuratePackageType.NSZ => fileInfo.Type switch
            {
                TitleLibraryType.Base   => _settings.NszBasePath,
                TitleLibraryType.Update => _settings.NszUpdatePath,
                TitleLibraryType.DLC    => _settings.NszDlcPath,
                _                       => string.Empty
            },
            AccuratePackageType.XCI => fileInfo.Type switch
            {
                TitleLibraryType.Base   => _settings.XciBasePath,
                TitleLibraryType.Update => _settings.XciUpdatePath,
                TitleLibraryType.DLC    => _settings.XciDlcPath,
                _                       => string.Empty
            },
            AccuratePackageType.XCZ => fileInfo.Type switch
            {
                TitleLibraryType.Base   => _settings.XczBasePath,
                TitleLibraryType.Update => _settings.XczUpdatePath,
                TitleLibraryType.DLC    => _settings.XczDlcPath,
                _                       => string.Empty
            },
            _ => throw new Exception("Unknown package type")
        };
        
        return await TemplateReplaceAsync(renameTemplate, fileInfo);

    }


    public async Task<string> CalculateSampleFileName(string templateText, PackageTitleType packageType, string inputFile, string basePath)
    {
        var libraryType = packageType switch
        {
            PackageTitleType.NspBase   => TitleLibraryType.Base,
            PackageTitleType.NspUpdate => TitleLibraryType.Update,
            PackageTitleType.NspDlc    => TitleLibraryType.DLC,
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

    public Task<RenamerSettings> SaveRenamerSettingsAsync(RenamerSettings settings)
    {
        _settings = settings;
        return Task.FromResult(_dataService.SaveRenamerSettings(settings));
    }

    public Task<RenamerSettings> LoadRenamerSettingsAsync()
    {
        _settings = _dataService.GetRenamerSettings();
        return Task.FromResult(_settings);
    }

    public async Task<FluentValidation.Results.ValidationResult> ValidateRenamerSettingsAsync(RenamerSettings settings)
    {
        var validationResult = await _validator.ValidateAsync(settings);
        return validationResult;
    }
    
    private static string TokenReplace(string input, string pattern, string replacement)
    {
        return Regex.Replace(input, pattern, replacement, RegexOptions.IgnoreCase);
    }
}