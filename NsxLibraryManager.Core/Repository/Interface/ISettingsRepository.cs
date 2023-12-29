using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Core.Repository.Interface;

public interface ISettingsRepository
{
    public RenamerSettings LoadRenamerSettings();
    
    public RenamerSettings SaveRenamerSettings(RenamerSettings settings);
}