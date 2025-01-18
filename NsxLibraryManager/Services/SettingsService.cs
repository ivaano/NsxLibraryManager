using System.Text.Json;
using Common.Contracts;
using Microsoft.EntityFrameworkCore;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Settings;
using NsxLibraryManager.Data;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Services.Interface;
using Settings = NsxLibraryManager.Models.NsxLibrary.Settings;

namespace NsxLibraryManager.Services;

public class SettingsService(NsxLibraryDbContext nsxLibraryDbContext, IConfiguration configuration) : ISettingsService, ISettingsIvan
{
    private static readonly Dictionary<SettingsEnum, string> InMemoryData = new();

    private readonly NsxLibraryDbContext _nsxLibraryDbContext = nsxLibraryDbContext ?? throw new ArgumentNullException(nameof(nsxLibraryDbContext));
    private static readonly object Lock = new();
    private Dictionary<SettingsEnum, string> _data = new();
    private IConfiguration _configuration = configuration;
    
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

    public UserSettings GetUserSettings()
    {

        Load();
        var libraryDbPath = _configuration.GetSection("NsxLibraryManager:NsxLibraryDbConnection").Value.CleanDatabasePath();
        var titleDbPath = _configuration.GetSection("NsxLibraryManager:TitledbDbConnection").Value.CleanDatabasePath();
        var tempUserSettings = new UserSettings
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
            LibraryPath = "temp",
            Recursive = true
        };
        _data.TryGetValue(SettingsEnum.UserSettings, out var serializedUserSettings);
        if (serializedUserSettings is null) return tempUserSettings;
        try
        {
            var userSettings = JsonSerializer.Deserialize<UserSettings>(serializedUserSettings);
            if (userSettings is null) return userSettings ?? tempUserSettings;
            userSettings.LibraryDatabase = libraryDbPath; //get library path from config file 
            userSettings.TitleDatabase = titleDbPath;

            return userSettings ?? tempUserSettings;
        }
        catch (JsonException ex)
        {
            return tempUserSettings;
        }

    }

    public string GetSettingByType(SettingsEnum settingEnumType)
    {
        Load();
        return _data.TryGetValue(settingEnumType, out var value) ? value : string.Empty;
    }

    public UserSettingValue GetSettingAsync(string key)
    {
        var userSettings = GetUserSettings();
        return new UserSettingValue("Prodkeys", userSettings.ProdKeys);  
    }
}