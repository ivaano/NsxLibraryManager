using NsxLibraryManager.Data;
using NsxLibraryManager.Models.NsxLibrary;

namespace NsxLibraryManager.Providers;

public class LiveDatabaseConfigurationProvider : ConfigurationProvider
{
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly Dictionary<string, string> _inMemoryData = new();
    private static readonly object _lock = new();

    public LiveDatabaseConfigurationProvider(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public override void Load()
    {
        lock (_lock)
        {
            if (!_inMemoryData.Any())
            {
                LoadFromDatabase();
            }
            Data = new Dictionary<string, string>(_inMemoryData);
        }
    }

    private void LoadFromDatabase()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NsxLibraryDbContext>();
        var dbData = dbContext.Setting.ToDictionary(s => s.Key, s => s.Value);
        
        _inMemoryData.Clear();
        foreach (var item in dbData)
        {
            _inMemoryData[item.Key] = item.Value;
        }
    }

    public void SetValue(string key, string value)
    {
        lock (_lock)
        {
            // Update database
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<NsxLibraryDbContext>();
            
            var setting = dbContext.Setting.FirstOrDefault(s => s.Key == key) 
                          ?? new Setting { Key = key };
            setting.Value = value;
            
            dbContext.Setting.Update(setting);
            dbContext.SaveChanges();

            // Update in-memory data
            _inMemoryData[key] = value;
            Data = new Dictionary<string, string>(_inMemoryData);
        }
    }
}