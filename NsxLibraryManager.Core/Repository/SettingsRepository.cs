using LiteDB;
using NsxLibraryManager.Core.Repository.Interface;
using NsxLibraryManager.Core.Settings;
namespace NsxLibraryManager.Core.Repository;

public sealed class SettingsRepository : BaseRepository<Models.Dto.Settings>, ISettingsRepository
{
    public SettingsRepository(ILiteDatabase db) : base(db, collectionName: AppConstants.SettingsCollectionName)
    {
    }
    
    public SettingsRepository(ILiteDatabase db, string collection) : base(db, collectionName: AppConstants.SettingsCollectionName)
    {
        SetCollection(collection);
    }
    
    public override Models.Dto.Settings Create(Models.Dto.Settings entity)
    {
        Collection.Insert(entity);
        Collection.EnsureIndex(x => x.Name);
        return Collection.Find(o => o.Name == entity.Name).FirstOrDefault() ?? throw new InvalidOperationException();
    }

    public RenamerSettings SaveRenamerSettings(RenamerSettings settings)
    {
        var renamerSettings = Collection.FindOne(o => o.Name == AppConstants.RenamerSettingsName);
        renamerSettings.Value = System.Text.Json.JsonSerializer.Serialize(settings);
        Update(renamerSettings);
        return LoadRenamerSettings();
    }

    public RenamerSettings LoadRenamerSettings()
    {
        var renamerSettings = Collection.FindOne(o => o.Name == AppConstants.RenamerSettingsName) 
                              ?? new Models.Dto.Settings { Name = AppConstants.RenamerSettingsName, Value = System.Text.Json.JsonSerializer.Serialize(new RenamerSettings()) };

        if (renamerSettings.Id == 0)
            Create(renamerSettings);

        return System.Text.Json.JsonSerializer.Deserialize<RenamerSettings>(renamerSettings.Value) ?? new RenamerSettings();        
    }
}