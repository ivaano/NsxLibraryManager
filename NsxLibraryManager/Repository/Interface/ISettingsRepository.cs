using Radzen;

namespace NsxLibraryManager.Repository.Interface;

public interface ISettingsRepository
{
    public Task SaveDataGridStateAsync(string name, DataGridSettings settings);
    
    public Task<DataGridSettings?> LoadDataGridStateAsync(string name);
}