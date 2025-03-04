using Common.Services;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using NsxLibraryManager.Shared.Settings;

namespace NsxLibraryManager.Services.Interface;

public interface ISettingsService
{
    public Task<BundleRenamerSettings> GetBundleRenamerSettings();
    public Task<CollectionRenamerSettings> GetCollectionRenamerSettings();
    public Task<PackageRenamerSettings> GetPackageRenamerSettings();
    public Task<FtpClientSettings> GetFtpClientSettings();
    public Task<PackageRenamerSettings> SavePackageRenamerSettings(PackageRenamerSettings packageRenamerSettings);
    public Task<BundleRenamerSettings> SaveBundleRenamerSettings(BundleRenamerSettings bundleRenamerSettings);
    public Task<CollectionRenamerSettings> SaveCollectionRenamerSettings(CollectionRenamerSettings collectionRenamerSettings);
    public Task<FtpClientSettings> SaveFtpClientSettings(FtpClientSettings settings);
    public Task<Result<byte[]>> ExportUserData();
    public Task<Result<int>> ImportUserData(string filePath);
    public string GetSettingByType(SettingsEnum key);
    
    public UserSettings GetUserSettings();
    
    public bool SaveUserSettings(UserSettings userSettings);
    
    public string GetConfigFolder();

    public bool RemoveCurrentKeys();
}