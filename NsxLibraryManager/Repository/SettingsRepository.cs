using LiteDB;
using NsxLibraryManager.Core.Models.Dto;
using NsxLibraryManager.Core.Repository;
using NsxLibraryManager.Repository.Interface;
using Radzen;

namespace NsxLibraryManager.Repository;

public class SettingsRepository : BaseRepository<Settings>, ISettingsRepository
{
    public SettingsRepository(ILiteDatabase db) : base(db)
    {
    }

    public SettingsRepository(ILiteDatabase db, string collectionName) : base(db, collectionName: "settings")
    {
    }
    
    public override Settings Create(Settings entity)
    {
        Collection.Insert(entity);
        Collection.EnsureIndex(x => x.Name);
        return Collection.Find(o => o.Name == entity.Name).FirstOrDefault() ?? throw new InvalidOperationException();
    }

    public Task SaveDataGridStateAsync(string name, DataGridSettings dataGridSettings)
    {
        var settings = new Settings
        {
                Name = name,
                Value = System.Text.Json.JsonSerializer.Serialize(dataGridSettings)
        };
        Create(settings);
        return Task.CompletedTask;
    }

    public Task<DataGridSettings?> LoadDataGridStateAsync(string name)
    {
        var dbGridSettings = Collection.Find(o => o.Name == name).FirstOrDefault();
        return dbGridSettings == null ? 
                Task.FromResult<DataGridSettings?>(null) : 
                Task.FromResult(System.Text.Json.JsonSerializer.Deserialize<DataGridSettings>(dbGridSettings.Value));
    }
}