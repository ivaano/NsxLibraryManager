using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using NsxLibraryManager.Shared.Settings;

namespace NsxLibraryManager.Core.Validators;

public class AppSettingsValidator: AbstractValidator<AppSettings>
{
    
    public static (bool valid, bool defaultConfigCreated) ValidateRootConfig(IConfigurationRoot configurationRoot)
    {
        if (File.Exists(configurationRoot[$"{AppConstants.AppSettingsSectionName}:{AppConstants.AppSettingsTitledbDbConnection}"]))
            return (true, false);

        if (configurationRoot[$"{AppConstants.AppSettingsSectionName}:{AppConstants.AppSettingsTitledbDbConnection}"] is null)
        {
            CreateDefaultConfigFile(AppConstants.ConfigFileName); 
            return (true, true);
        }

        return (true, false);

    }
    
    
    private static bool CreateDefaultConfigFile(string configFileName)
    {
        var jsonWriteOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        jsonWriteOptions.Converters.Add(new JsonStringEnumConverter());

        var appSettings = new AppSettings
        {
            NsxLibraryDbConnection =
                $"Data Source={Path.Combine(AppContext.BaseDirectory, AppConstants.DataDirectory, AppConstants.DefaultNsxLibraryDb)}",
            TitledbDbConnection =
                $"Data Source={Path.Combine(AppContext.BaseDirectory, AppConstants.DataDirectory, AppConstants.DefaultTitleDbName)}",
            SqlTitleDbRepository = AppConstants.DefaultTitleRepository,
        };
        
        var sectionName = new Dictionary<string, AppSettings>
        {
            { AppConstants.AppSettingsSectionName, appSettings }
        };
        
        var newJson = JsonSerializer.Serialize(sectionName, jsonWriteOptions);
        var configFolder = Path.Combine(AppContext.BaseDirectory, AppConstants.ConfigDirectory);
        if (!Directory.Exists(configFolder))
        {
            Directory.CreateDirectory(configFolder);
        }
        var dataFolder = Path.Combine(AppContext.BaseDirectory, AppConstants.DataDirectory);
        if (!Directory.Exists(dataFolder))
        {
            Directory.CreateDirectory(dataFolder);
        }

        var appSettingsPath = Path.Combine(configFolder, AppConstants.ConfigFileName);
        File.WriteAllText(appSettingsPath, newJson);

        return false;
    }
    
}