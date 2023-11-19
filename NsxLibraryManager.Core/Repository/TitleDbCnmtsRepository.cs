using LiteDB;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Repository.Interface;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Core.Repository;

public class TitleDbCnmtsRepository : BaseRepository<PackagedContentMeta>, ITitleDbCnmtsRepository
{
    public TitleDbCnmtsRepository(ILiteDatabase db) : base(db, collectionName: AppConstants.CnmtsCollectionName)
    {
    }
    
    public int InsertBulk(IEnumerable<PackagedContentMeta> entities)
    {
        var resultCount = Collection.InsertBulk(entities);
        Collection.EnsureIndex(x => x.TitleId);
        Collection.EnsureIndex(x => x.OtherApplicationId);
        return resultCount;
    }
    
    public IEnumerable<PackagedContentMeta> FindByOtherApplicationId(string titleId)
    {
        return Collection.Find(x => x.OtherApplicationId == titleId);
    }
}