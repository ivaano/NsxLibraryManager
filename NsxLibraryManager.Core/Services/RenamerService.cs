using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NsxLibraryManager.Core.Enums;
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

    public RenamerService(ILogger<RenamerService> logger,
        IDataService dataService,
        IValidator<RenamerSettings> validator,
        IFileInfoService fileInfoService)
    {
        _logger = logger;
        _dataService = dataService;
        _validator = validator;
        _fileInfoService = fileInfoService;
    }

    private async Task<string> TemplateReplaceAsync(string renameTemplate, LibraryTitle fileInfo)
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
            
            newPath = TokenReplace(renameTemplate, pattern, replacement);
            renameTemplate = newPath;
        }
        return newPath;
    }

    public async Task<IEnumerable<RenameTitle>> GetFilesToRenameAsync(string inputPath, bool recursive = false)
    {
        var files = await _fileInfoService.GetFileNames(inputPath, recursive);
        var fileList = new List<RenameTitle>();
        
        foreach (var file in files)
        {

            var fileInfo = await _fileInfoService.GetFileInfo(file, false);
            var error = false;
            var newPath = string.Empty;
            try
            {
                newPath = await BuildNewFileNameAsync(fileInfo, file);    
            } catch (Exception e)
            {
                _logger.LogError(e, "Error building new file name");
                error = true;
            }
            
            
            var newRenameTitle = new RenameTitle(file, newPath, fileInfo.TitleId, fileInfo.TitleName, error);
            fileList.Add(newRenameTitle);
        }
        
        return fileList;
    }

    public async Task<string> BuildNewFileNameAsync(LibraryTitle fileInfo, string filePath)
    {
        //var fileInfo = await _fileInfoService.GetFileInfo(filePath, false);
        fileInfo.FileName = filePath;

        var newPath = string.Empty;

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


            newPath = TokenReplace(renameTemplate, pattern, replacement);
            renameTemplate = newPath;
        }
        return newPath;
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
        var filePath = "c:/test/algo.nsp";
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