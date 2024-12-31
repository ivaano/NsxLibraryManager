using Microsoft.EntityFrameworkCore;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Settings;
using NsxLibraryManager.Data;
using NsxLibraryManager.Services.Interface;
using Radzen;
using Settings = NsxLibraryManager.Models.NsxLibrary.Settings;

namespace NsxLibraryManager.Services;

public class SettingsService(NsxLibraryDbContext nsxLibraryDbContext) : ISettingsService
{
    private readonly NsxLibraryDbContext _nsxLibraryDbContext = nsxLibraryDbContext ?? throw new ArgumentNullException(nameof(nsxLibraryDbContext));


    private static PackageRenamerSettings MapToPackageRenamerSettings(Settings renamerSettings)
    {
        return System.Text.Json.JsonSerializer
            .Deserialize<PackageRenamerSettings>(renamerSettings.Value) ?? new PackageRenamerSettings();        
    }
    
    private static BundleRenamerSettings MapToBundleRenamerSettings(Settings renamerSettings)
    {
        return System.Text.Json.JsonSerializer
            .Deserialize<BundleRenamerSettings>(renamerSettings.Value) ?? new BundleRenamerSettings();        
    }
    
    public async Task<BundleRenamerSettings> GetBundleRenamerSettings()
    {
        var setttings = await _nsxLibraryDbContext.Settings
            .FirstOrDefaultAsync(c => c.Setting == Core.Enums.Settings.RenameBundle);
        
        return setttings == null ? new BundleRenamerSettings() : 
            MapToBundleRenamerSettings(setttings);
    }
    
    public async Task<PackageRenamerSettings> GetPackageRenamerSettings()
    {
        var setttings = await _nsxLibraryDbContext.Settings
            .FirstOrDefaultAsync(c => c.Setting == Core.Enums.Settings.RenameType);
        
        return setttings == null ? new PackageRenamerSettings() : 
            MapToPackageRenamerSettings(setttings);
    }

    public async Task<PackageRenamerSettings> SavePackageRenamerSettings(PackageRenamerSettings packageRenamerSettings)
    { 
        var serializedSettings = System.Text.Json.JsonSerializer.Serialize(packageRenamerSettings);
        
        var setttings = await _nsxLibraryDbContext.Settings
            .FirstOrDefaultAsync(c => c.Setting == Core.Enums.Settings.RenameType);

        if (setttings is null)
        {
            var settingsInDb = new Settings
            {
                Setting = Core.Enums.Settings.RenameType,
                Value = serializedSettings
            };
            _nsxLibraryDbContext.Settings.Add(settingsInDb);

        }
        else
        {
            setttings.Value = serializedSettings;
        }
        
        await _nsxLibraryDbContext.SaveChangesAsync();
        return packageRenamerSettings;
    }
}