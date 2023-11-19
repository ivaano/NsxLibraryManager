using LiteDB;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Repository.Interface;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Core.Repository;

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
        return Collection.Find(x => x.TitleId == titleId).OrderBy(x => x.VersionShifted);
    }
}
