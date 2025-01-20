using System.Text.Json;
using Common.Contracts;
using Microsoft.EntityFrameworkCore;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Services.KeysManagement;
using NsxLibraryManager.Core.Settings;
using NsxLibraryManager.Data;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Utils;
using Settings = NsxLibraryManager.Models.NsxLibrary.Settings;

namespace NsxLibraryManager.Services;

public class SettingsService(
    NsxLibraryDbContext nsxLibraryDbContext, 
    IConfiguration configuration) : ISettingsService, ISettingsMediator
{
    private static readonly Dictionary<SettingsEnum, string> InMemoryData = new();
    private readonly NsxLibraryDbContext _nsxLibraryDbContext = nsxLibraryDbContext ?? throw new ArgumentNullException(nameof(nsxLibraryDbContext));
    private static readonly object Lock = new();
    private Dictionary<SettingsEnum, string> _data = new();
     
    
    private void Load()
    {
        lock (Lock)
        {
            if (InMemoryData.Count == 0)
            {
                LoadFromDatabase();
            }
            _data = new Dictionary<SettingsEnum, string>(InMemoryData);
        }
    }

    private UserSettings GetDefaultSettings()
    {
        var libraryDbPath = configuration.GetSection("NsxLibraryManager:NsxLibraryDbConnection").Value.CleanDatabasePath();
        var titleDbPath = configuration.GetSection("NsxLibraryManager:TitledbDbConnection").Value.CleanDatabasePath();

        
        return new UserSettings
        {
            DownloadSettings = new DownloadSettings
            {
                TimeoutInSeconds = 60,
                TitleDbPath = Path.Combine(AppContext.BaseDirectory, AppConstants.DataDirectory),
                VersionUrl = AppConstants.DefaultTitleDbVersion,
                TitleDbUrl = AppConstants.DefaultTitleDbUrl
            },
            TitleDatabase = titleDbPath,
            LibraryDatabase = libraryDbPath,
            ProdKeys = GetKeyLocation(IKeySetProviderService.DefaultProdKeysFileName),
            TitleKeys = GetKeyLocation(IKeySetProviderService.DefaultTitleKeysFileName),
            ConsoleKeys = GetKeyLocation(IKeySetProviderService.DefaultConsoleKeysFileName),
            LibraryPath = "your_library_path",
            Recursive = true
        };
    }
    
    private void LoadFromDatabase()
    {
        var dbData = _nsxLibraryDbContext.Settings.ToDictionary(s => s.Key, s => s.Value);
        
        InMemoryData.Clear();
        foreach (var item in dbData)
        {
            InMemoryData[item.Key] = item.Value;
        }
        _data = new Dictionary<SettingsEnum, string>(InMemoryData);
    }
    
    private void SetValue(SettingsEnum settingEnumType, string value)
    {
        lock (Lock)
        {
            var setting = _nsxLibraryDbContext.Settings.FirstOrDefault(s => s.Key == settingEnumType) 
                          ?? new Settings
            {
                Key = settingEnumType,
                Value = value
            };
            setting.Value = value;
            
            _nsxLibraryDbContext.Settings.Update(setting);
            _nsxLibraryDbContext.SaveChanges();

            InMemoryData[settingEnumType] = value;
        }
    }
    
    private static T MapToSettings<T>(Settings settings) where T : new()
    {
        return JsonSerializer.Deserialize<T>(settings.Value) ?? new T();
    }

    private async Task<T> GetSerializedSettings<T>(SettingsEnum settingEnumType) where T : new()
    {
        var settings = await _nsxLibraryDbContext.Settings
            .FirstOrDefaultAsync(c => c.Key == settingEnumType);

        return settings is not null ? MapToSettings<T>(settings) : new T();
    }
    
    private async Task<T> SaveSerializedSettings<T>(T settings, SettingsEnum settingEnumType)
    {
        var serializedSettings = JsonSerializer.Serialize(settings);
        var existingSettings = await _nsxLibraryDbContext.Settings
            .FirstOrDefaultAsync(c => c.Key == settingEnumType);

        if (existingSettings is null)
        {
            _nsxLibraryDbContext.Settings.Add(new Settings
            {
                Key = settingEnumType,
                Value = serializedSettings
            });
        }
        else
        {
            existingSettings.Value = serializedSettings;
        }

        await _nsxLibraryDbContext.SaveChangesAsync();
        return settings;
    }

    public Task<BundleRenamerSettings> GetBundleRenamerSettings() =>
        GetSerializedSettings<BundleRenamerSettings>(SettingsEnum.RenameBundle);
    
    public Task<BundleRenamerSettings> SaveBundleRenamerSettings(BundleRenamerSettings settings) =>
        SaveSerializedSettings(settings, SettingsEnum.RenameBundle);
    
    public Task<PackageRenamerSettings> GetPackageRenamerSettings() =>
        GetSerializedSettings<PackageRenamerSettings>(SettingsEnum.RenamePackageType);
    
    public Task<PackageRenamerSettings> SavePackageRenamerSettings(PackageRenamerSettings settings) =>
        SaveSerializedSettings(settings, SettingsEnum.RenamePackageType);

    public bool SaveUserSettings(UserSettings userSettings)
    {
        var serializedSettings = JsonSerializer.Serialize(userSettings);
        SetValue(SettingsEnum.UserSettings, serializedSettings);
        return true;
    }

    public string GetConfigFolder()
    {
        return Path.Combine(AppContext.BaseDirectory, AppConstants.ConfigDirectory);
    }

    public bool RemoveCurrentKeys()
    {
        var prodKeys = Path.Combine(PathHelper.CurrentAppDir, AppConstants.ConfigDirectory, IKeySetProviderService.DefaultProdKeysFileName);
        var titleKeys = Path.Combine(PathHelper.CurrentAppDir, AppConstants.ConfigDirectory, IKeySetProviderService.DefaultTitleKeysFileName);
        var consoleKeys = Path.Combine(PathHelper.CurrentAppDir, AppConstants.ConfigDirectory, IKeySetProviderService.DefaultConsoleKeysFileName);

        if (File.Exists(prodKeys))
        {
            File.Delete(prodKeys);
        }

        if (File.Exists(titleKeys))
        {
            File.Delete(titleKeys);
        }

        if (File.Exists(consoleKeys))
        {
            File.Delete(consoleKeys);
        }
            
        return true;
    }

    public UserSettings GetUserSettings()
    {
        Load();
        _data.TryGetValue(SettingsEnum.UserSettings, out var serializedUserSettings);
        if (serializedUserSettings is null) return GetDefaultSettings();
        try
        {
            var userSettings = JsonSerializer.Deserialize<UserSettings>(serializedUserSettings);
            if (userSettings is null) return userSettings ?? GetDefaultSettings();
            userSettings.LibraryDatabase = configuration
                .GetSection("NsxLibraryManager:NsxLibraryDbConnection").Value
                .CleanDatabasePath();
            userSettings.TitleDatabase = configuration
                .GetSection("NsxLibraryManager:TitledbDbConnection").Value
                .CleanDatabasePath();

            userSettings.ProdKeys = GetKeyLocation(IKeySetProviderService.DefaultProdKeysFileName);
            userSettings.TitleKeys = GetKeyLocation(IKeySetProviderService.DefaultTitleKeysFileName);
            userSettings.ConsoleKeys = GetKeyLocation(IKeySetProviderService.DefaultConsoleKeysFileName);
            
            
            return userSettings;
        }
        catch (JsonException ex)
        {
            return GetDefaultSettings();
        }

    }

    public string GetSettingByType(SettingsEnum settingEnumType)
    {
        Load();
        return _data.TryGetValue(settingEnumType, out var value) ? value : string.Empty;
    }

    private string GetKeyLocation(string keysFileName)
    {
        var appDirKeysFilePath = Path.Combine(PathHelper.CurrentAppDir, AppConstants.ConfigDirectory, keysFileName);
        if (File.Exists(appDirKeysFilePath))
            return appDirKeysFilePath;

        var homeUserDir = PathHelper.HomeUserDir;
        if (homeUserDir is null) return string.Empty;
        var homeDirKeysFilePath = Path.Combine(homeUserDir, ".switch", keysFileName).ToFullPath();
        return File.Exists(homeDirKeysFilePath) ? homeDirKeysFilePath : string.Empty;
    }

    public UserSettingValue GetUserSetting(string key)
    {
        var userSettings = GetUserSettings();
        return key.ToLower() switch
        {
            "prodkeys" => new UserSettingValue(key, userSettings.ProdKeys),
            "titlekeys" => new UserSettingValue(key, userSettings.TitleKeys),
            "consolekeys" => new UserSettingValue(key, userSettings.ConsoleKeys),
            _ => new UserSettingValue(key, string.Empty)
        };
    }
}