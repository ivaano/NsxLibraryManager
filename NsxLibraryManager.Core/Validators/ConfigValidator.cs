using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Core.Validators;

public static class ConfigValidator
{
    public static bool Validate(IConfigurationRoot configurationManager)
    {
        if (File.Exists(configurationManager["NsxLibraryManager:TitleDatabase"]))
            return true;
        
        CreateDefaultConfigFile(AppConstants.ConfigFileName);
        return true;
    }
    
    public static bool Validate(IOptions<AppSettings> configAppSettings)
    {
        //builder.Configuration["NsxLibraryManager:LibraryPath"] != string.Empty || builder.Configuration["NsxLibraryManager:LibraryPath"] is not null
        return true;
    }
    
    public static bool CreateDefaultConfigFile(string configFileName)
    {
        var jsonWriteOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        jsonWriteOptions.Converters.Add(new JsonStringEnumConverter());

        var appSettings = new AppSettings()
        {
            TitleDatabase = Path.Combine(AppContext.BaseDirectory, "title.db"),
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
        
        var newJson = JsonSerializer.Serialize(sectionName, jsonWriteOptions);
        var appSettingsPath = Path.Combine(AppContext.BaseDirectory, AppConstants.ConfigFileName);
        File.WriteAllText(appSettingsPath, newJson);
        
        return false;
    }
}