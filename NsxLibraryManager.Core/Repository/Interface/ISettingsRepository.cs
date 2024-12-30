using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Core.Repository.Interface;

public interface ISettingsRepository
{
    public PackageRenamerSettings LoadRenamerSettings();
    
    public PackageRenamerSettings SaveRenamerSettings(PackageRenamerSettings settings);
}