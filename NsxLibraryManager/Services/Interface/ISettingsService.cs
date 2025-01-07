using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Services.Interface;

public interface ISettingsService
{
    public Task<BundleRenamerSettings> GetBundleRenamerSettings();
    public Task<PackageRenamerSettings> GetPackageRenamerSettings();
    public Task<PackageRenamerSettings> SavePackageRenamerSettings(PackageRenamerSettings packageRenamerSettings);
    public Task<BundleRenamerSettings> SaveBundleRenamerSettings(BundleRenamerSettings bundleRenamerSettings);
    
    public string GetSettingByType(SettingsEnum key);
    
    public UserSettings GetUserSettings();
    
    public bool SaveUserSettings(UserSettings userSettings);
}