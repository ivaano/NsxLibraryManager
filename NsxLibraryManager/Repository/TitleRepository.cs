using LiteDB;
using NsxLibraryManager.Models;


namespace NsxLibraryManager.Repository;

public class TitleRepository : BaseRepository<TitleDbTitle>, ITitleRepository
{
    public TitleRepository(ILiteDatabase db) : base(db, collectionName: "titles")
    {
    }

    public override TitleDbTitle Create(TitleDbTitle entity)
    {
        var now = DateTime.Now;
        entity.CreatedTime = now;
        entity.ModifiedTime = now;
        var newId = Collection.Insert(entity);
        
        Collection.EnsureIndex(x => x.nsuId);

        return Collection.Find(o => o.nsuId == entity.nsuId)?.FirstOrDefault() ?? throw new InvalidOperationException();
    }
    
    public override void Update(TitleDbTitle entity)
    {
        entity.ModifiedTime = DateTime.Now;
        Collection.Upsert(entity);
        Collection.EnsureIndex(x => x.id);
    }

    public TitleDbTitle? GetTitleById(string id)
    {
        return Collection.Find(o => o.id == id)?.FirstOrDefault();
    }
}