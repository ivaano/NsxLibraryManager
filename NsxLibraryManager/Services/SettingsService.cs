using Microsoft.EntityFrameworkCore;
using NsxLibraryManager.Core.Settings;
using NsxLibraryManager.Data;
using NsxLibraryManager.Services.Interface;
using Settings = NsxLibraryManager.Models.NsxLibrary.Settings;

namespace NsxLibraryManager.Services;

public class SettingsService(NsxLibraryDbContext nsxLibraryDbContext) : ISettingsService
{
    private readonly NsxLibraryDbContext _nsxLibraryDbContext = nsxLibraryDbContext ?? throw new ArgumentNullException(nameof(nsxLibraryDbContext));

    private static T MapToSettings<T>(Settings settings) where T : new()
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(settings.Value) ?? new T();
    }

    private async Task<T> GetSettings<T>(Core.Enums.Settings settingType) where T : new()
    {
        var settings = await _nsxLibraryDbContext.Settings
            .FirstOrDefaultAsync(c => c.Setting == settingType);

        return settings is not null ? MapToSettings<T>(settings) : new T();
    }
    
    private async Task<T> SaveSettings<T>(T settings, Core.Enums.Settings settingType)
    {
        var serializedSettings = System.Text.Json.JsonSerializer.Serialize(settings);
        var existingSettings = await _nsxLibraryDbContext.Settings
            .FirstOrDefaultAsync(c => c.Setting == settingType);

        if (existingSettings is null)
        {
            _nsxLibraryDbContext.Settings.Add(new Settings
            {
                Setting = settingType,
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
        GetSettings<BundleRenamerSettings>(Core.Enums.Settings.RenameBundle);

    public Task<PackageRenamerSettings> GetPackageRenamerSettings() =>
        GetSettings<PackageRenamerSettings>(Core.Enums.Settings.RenameType);
    
    public Task<BundleRenamerSettings> SaveBundleRenamerSettings(BundleRenamerSettings settings) =>
        SaveSettings(settings, Core.Enums.Settings.RenameBundle);

    public Task<PackageRenamerSettings> SavePackageRenamerSettings(PackageRenamerSettings settings) =>
        SaveSettings(settings, Core.Enums.Settings.RenameType);

}