using LiteDB;
using NsxLibraryManager.Models;
using NsxLibraryManager.Settings;

namespace NsxLibraryManager.Repository;

public class TitleDbVersionsRepository : BaseRepository<GameVersions>, ITitleDbVersionsRepository
{
    public TitleDbVersionsRepository(ILiteDatabase db) : base(db, collectionName: AppConstants.VersionsCollectionName)
    {
    }

    public int InsertBulk(IEnumerable<GameVersions> entities)
    {
        var resultCount = Collection.InsertBulk(entities);
        Collection.EnsureIndex(x => x.TitleId);
        return resultCount;
    }
    
    public IEnumerable<GameVersions> FindByTitleId(string titleId)
    {
        return Collection.Find(x => x.TitleId == titleId);
    }
}
