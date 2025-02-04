using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Services.Interface;

public interface ISettingsService
{
    public Task<BundleRenamerSettings> GetBundleRenamerSettings();
    public Task<CollectionRenamerSettings> GetCollectionRenamerSettings();
    public Task<PackageRenamerSettings> GetPackageRenamerSettings();
    public Task<PackageRenamerSettings> SavePackageRenamerSettings(PackageRenamerSettings packageRenamerSettings);
    public Task<BundleRenamerSettings> SaveBundleRenamerSettings(BundleRenamerSettings bundleRenamerSettings);
    public Task<CollectionRenamerSettings> SaveCollectionRenamerSettings(CollectionRenamerSettings collectionRenamerSettings);
    
    public string GetSettingByType(SettingsEnum key);
    
    public UserSettings GetUserSettings();
    
    public bool SaveUserSettings(UserSettings userSettings);
    
    public string GetConfigFolder();

    public bool RemoveCurrentKeys();
}