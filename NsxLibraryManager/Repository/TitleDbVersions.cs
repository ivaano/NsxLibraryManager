using LiteDB;
using NsxLibraryManager.Models;
using NsxLibraryManager.Settings;

namespace NsxLibraryManager.Repository;

public class TitleDbVersions : BaseRepository<GameVersions>, ITitleDbVersions
{
    public TitleDbVersions(ILiteDatabase db) : base(db, collectionName: AppConstants.VersionsCollectionName)
    {
    }

    public int InsertBulk(IEnumerable<GameVersions> entities)
    {
        var resultCount = Collection.InsertBulk(entities);
        Collection.EnsureIndex(x => x.TitleId);
        return resultCount;
    }
}
