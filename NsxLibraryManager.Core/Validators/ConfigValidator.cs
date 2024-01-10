using System.Text.Json;
using System.Text.Json.Serialization;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NsxLibraryManager.Core.Settings;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace NsxLibraryManager.Core.Validators;

public static class ConfigValidator
{
    public static (bool valid, bool defaultConfigCreated) Validate(IConfigurationRoot configurationRoot)
    {
        if (File.Exists(configurationRoot["NsxLibraryManager:TitleDatabase"]))
            return (true, false);
        
        CreateDefaultConfigFile(AppConstants.ConfigFileName);
        return (true, true);
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
        
        var newJson = JsonSerializer.Serialize(sectionName, jsonWriteOptions);
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
        return false;
    }
}