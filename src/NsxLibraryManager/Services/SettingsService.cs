using System.Globalization;
using System.Text.Json;
using Common.Contracts;
using Common.Services;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using NsxLibraryManager.Core.Services.KeysManagement;
using NsxLibraryManager.Data;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Models.NsxLibrary;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using NsxLibraryManager.Shared.Settings;
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
        var inContainer = bool.TryParse(configuration["DOTNET_RUNNING_IN_CONTAINER"], out var value) && value;
        var defaultLibraryPath = inContainer ? "/app/library" : "your_library_path";
        var defaultBackupPath = inContainer ? "/app/backup" : "your_backup_path";
        
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
            LibraryPath = defaultLibraryPath,
            BackupPath = defaultBackupPath,
            Recursive = true,
            IsConfigured = false
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

    public Task<CollectionRenamerSettings> GetCollectionRenamerSettings() =>
        GetSerializedSettings<CollectionRenamerSettings>(SettingsEnum.RenameCollection);

    public Task<PackageRenamerSettings> GetPackageRenamerSettings() =>
        GetSerializedSettings<PackageRenamerSettings>(SettingsEnum.RenamePackageType);
    public Task<FtpClientSettings> GetFtpClientSettings() =>
        GetSerializedSettings<FtpClientSettings>(SettingsEnum.FtpClientSettings);
    
    public Task<FtpClientSettings> SaveFtpClientSettings(FtpClientSettings settings) =>
        SaveSerializedSettings(settings, SettingsEnum.FtpClientSettings);
    
    public Task<BundleRenamerSettings> SaveBundleRenamerSettings(BundleRenamerSettings settings) =>
        SaveSerializedSettings(settings, SettingsEnum.RenameBundle);

    public Task<CollectionRenamerSettings> SaveCollectionRenamerSettings(CollectionRenamerSettings settings)=>
        SaveSerializedSettings(settings, SettingsEnum.RenameCollection);
    
    public Task<PackageRenamerSettings> SavePackageRenamerSettings(PackageRenamerSettings settings) =>
        SaveSerializedSettings(settings, SettingsEnum.RenamePackageType);

    public bool SaveUserSettings(UserSettings userSettings)
    {
        userSettings.IsConfigured = true;
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
        catch (JsonException)
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
    
    /// <summary>
    /// ExportUser Data only exports user generated data, for now UserRating and Collections.
    /// </summary>
    /// <returns>Result</returns>
    public async Task<Result<byte[]>> ExportUserData()
    {
        //get titles with ratings or collections
        var titles = await _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .Include(x => x.Collection)
            .Where(x => x.UserRating > 0 || x.Collection != null)
            .OrderBy(t => t.ApplicationId)
            .Select(x => x.MapExportUserDataDto())
            .ToListAsync();

        var csvBytes = CsvGenerator.GenerateCsv(titles);
        return Result.Success(csvBytes);
    }

    private Result<List<ExportUserDataDto>> ReadUserDataCsv(string filePath)
    {
        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) 
            {
                HasHeaderRecord = true
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);
            return Result.Success(csv.GetRecords<ExportUserDataDto>().ToList()); 
        }
        catch (Exception ex)
        {
            return Result.Failure<List<ExportUserDataDto>>($"Error reading CSV: {ex.Message}");
        }
    }
    public async Task<Result<int>> ImportUserData(string filePath)
    {
        var readCsvResult = ReadUserDataCsv(filePath);
        if (readCsvResult.IsSuccess)
        {
            var userData = readCsvResult.Value;
            var noUpdateCount = 0;
            foreach (var row in userData)
            {
                var title = await _nsxLibraryDbContext.Titles.FirstOrDefaultAsync(x => x.ApplicationId == row.ApplicationId);
                if (title is null)
                {
                    noUpdateCount++;
                    continue;
                }

                title.UserRating = row.UserRating;
                if (!string.IsNullOrEmpty(row.Collection))
                {
                    //check collection exists
                    var collection =
                        await _nsxLibraryDbContext.Collections.FirstOrDefaultAsync(x => x.Name == row.Collection);
                    if (collection is not null)
                    {
                        collection.Titles.Add(title);
                    }
                    else
                    {
                        var newCollection = new Collection
                        {
                            Name = row.Collection
                        };
                        newCollection.Titles.Add(title);
                        _nsxLibraryDbContext.Collections.Add(newCollection);
                    }

                }
            }
        }
        var updateCount = await _nsxLibraryDbContext.SaveChangesAsync();

        return Result.Success(updateCount);
    }
    
}