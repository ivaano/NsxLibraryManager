using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Core.Services;

public class RenamerService : IRenamerService
{
    private readonly ILogger<RenamerService> _logger;
    private readonly RenamerSettings _settings = default!;
    private readonly IDataService _dataService;
    private readonly IValidator<RenamerSettings> _validator;
    
    public RenamerService(ILogger<RenamerService> logger, IDataService dataService, IValidator<RenamerSettings> validator)
    {
        _logger = logger;
        _dataService = dataService;
        _validator = validator;
    }


    public Task<string> CalculateSampleFileName(PackageTitleType type, string inputFile, string basePath)
    {
        var tango = "cash";
        return Task.FromResult(tango);
    }

    public Task<RenamerSettings> SaveRenamerSettingsAsync(RenamerSettings settings)
    { 
        return Task.FromResult(_dataService.SaveRenamerSettings(settings));
    }

    public Task<RenamerSettings> LoadRenamerSettingsAsync()
    {
        return Task.FromResult(_dataService.GetRenamerSettings());
    }

    public async Task<FluentValidation.Results.ValidationResult> ValidateRenamerSettingsAsync(RenamerSettings settings)
    {
        var validationResult = await _validator.ValidateAsync(settings);
        return validationResult;
    }
}