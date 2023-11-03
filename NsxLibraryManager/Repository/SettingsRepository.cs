using LiteDB;
using NsxLibraryManager.Repository.Interface;
using Radzen;

namespace NsxLibraryManager.Repository;

public class SettingsRepository : BaseRepository<Models.Dto.Settings>, ISettingsRepository
{
    public SettingsRepository(ILiteDatabase db) : base(db)
    {
    }

    public SettingsRepository(ILiteDatabase db, string collectionName) : base(db, collectionName: "settings")
    {
    }
    
    public override Models.Dto.Settings Create(Models.Dto.Settings entity)
    {
        Collection.Insert(entity);
        Collection.EnsureIndex(x => x.Name);
        return Collection.Find(o => o.Name == entity.Name).FirstOrDefault() ?? throw new InvalidOperationException();
    }

    public async Task SaveDataGridStateAsync(string name, DataGridSettings dataGridSettings)
    {
        var settings = new Models.Dto.Settings
        {
                Name = name,
                Value = System.Text.Json.JsonSerializer.Serialize(dataGridSettings)
        };
        Create(settings);
    }

    public Task<DataGridSettings?> LoadDataGridStateAsync(string name)
    {
        var dbGridSettings = Collection.Find(o => o.Name == name).FirstOrDefault();
        return dbGridSettings == null ? 
                Task.FromResult<DataGridSettings?>(null) : 
                Task.FromResult(System.Text.Json.JsonSerializer.Deserialize<DataGridSettings>(dbGridSettings.Value));
    }
}