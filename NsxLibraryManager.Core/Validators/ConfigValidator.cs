using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Core.Validators;

public class ConfigValidator : AbstractValidator<AppSettings>
{
    public ConfigValidator()
    {
        RuleFor(x => x.TitleDatabase)
            .NotEmpty().WithMessage("Title database path cannot be empty.");

        RuleFor(x => x.TitleDatabase)
            .Custom((path, context) =>
            {
                if (!File.Exists(path))
                {
                    context.AddFailure("Title database does not exist.");
                }
            });
        RuleFor(x => x.ProdKeys)
            .Custom((path, context) =>
            {
                if (path != string.Empty && !File.Exists(path))
                {
                    context.AddFailure("prod.keys file must exist or leave empty to look in home folder.");
                }
            });        

        RuleFor(x => x.LibraryPath)
            .NotEmpty().WithMessage("Library path cannot be empty.");

        RuleFor(x => x.LibraryPath)
            .Custom((path, context) =>
            {
                if (!Directory.Exists(path))
                {
                    context.AddFailure("Library path does not exist.");
                }
            });

        RuleFor(x => x.DownloadSettings)
            .NotNull().WithMessage("Download settings cannot be null.");

        RuleFor(x => x.DownloadSettings.TitleDbPath)
            .NotEmpty().WithMessage("TitleDb path cannot be empty.");

        RuleFor(x => x.DownloadSettings.TitleDbPath)
            .Custom((path, context) =>
            {
                if (!Directory.Exists(path))
                {
                    context.AddFailure("TitleDb path does not exist.");
                }
            });

        RuleFor(x => x.DownloadSettings.TimeoutInSeconds)
            .GreaterThan(0).WithMessage("Timeout must be greater than 0.");

        RuleFor(x => x.DownloadSettings.RegionUrl)
            .NotEmpty().WithMessage("Region url cannot be empty.");

        RuleFor(x => x.DownloadSettings.CnmtsUrl)
            .NotEmpty().WithMessage("Cnmts url cannot be empty.");

        RuleFor(x => x.DownloadSettings.VersionsUrl)
            .NotEmpty().WithMessage("Versions url cannot be empty.");

        RuleFor(x => x.DownloadSettings.Regions)
            .NotEmpty().WithMessage("Regions cannot be empty.");
    }
    
    public static (bool valid, bool defaultConfigCreated) ValidateRootConfig(IConfigurationRoot configurationRoot)
    {
        if (File.Exists(configurationRoot["NsxLibraryManager:TitleDatabase"]))
            return (true, false);

        if (configurationRoot["NsxLibraryManager:TitleDatabase"] is null)
        {
            CreateDefaultConfigFile(AppConstants.ConfigFileName); 
            return (true, true);
        }

        _ = new LiteDatabase(configurationRoot["NsxLibraryManager:TitleDatabase"]);
        return (true, false);

    }
    
    private static bool CreateDefaultConfigFile(string configFileName)
    {
        var jsonWriteOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        jsonWriteOptions.Converters.Add(new JsonStringEnumConverter());

        var appSettings = new AppSettings()
        {
            TitleDatabase = Path.Combine(AppContext.BaseDirectory, "NsxlibraryManager.db"),
            LibraryPath = Path.Combine(AppContext.BaseDirectory, "library"),
            DownloadSettings = new DownloadSettings()
            {
                TitleDbPath = Path.Combine(AppContext.BaseDirectory, "titledb"),
                TimeoutInSeconds = 100,
                RegionUrl = "https://raw.githubusercontent.com/blawar/titledb/master/{region}.en.json",
                CnmtsUrl = "https://raw.githubusercontent.com/blawar/titledb/master/cnmts.json",
                VersionsUrl = "https://raw.githubusercontent.com/blawar/titledb/master/versions.json",
                Regions = new List<string> {"US"}
            }
        };
        
        var sectionName = new Dictionary<string, AppSettings>
        {
            { AppConstants.AppSettingsSectionName, appSettings }
        };
        
        var newJson = System.Text.Json.JsonSerializer.Serialize(sectionName, jsonWriteOptions);
        var appSettingsPath = Path.Combine(AppContext.BaseDirectory, AppConstants.ConfigFileName);
        File.WriteAllText(appSettingsPath, newJson);
        //create an empty db file and default folders
        _ = new LiteDatabase(appSettings.TitleDatabase);
        if (!Directory.Exists(appSettings.LibraryPath))
        {
            Directory.CreateDirectory(appSettings.LibraryPath);
        }
        if (!Directory.Exists(appSettings.DownloadSettings.TitleDbPath))
        {
            Directory.CreateDirectory(appSettings.DownloadSettings.TitleDbPath);
        }
        var keysPath = Path.Combine(AppContext.BaseDirectory, "keys");
        if (!Directory.Exists(keysPath))
        {
            Directory.CreateDirectory(keysPath);
        }
        return false;
    }
}