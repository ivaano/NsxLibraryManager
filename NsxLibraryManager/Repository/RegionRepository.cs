using System.Linq.Expressions;
using LiteDB;
using LiteDB.Queryable;
using NsxLibraryManager.Models;


namespace NsxLibraryManager.Repository;

public sealed class RegionRepository : BaseRepository<RegionTitle>, IRegionRepository
{
    public RegionRepository(ILiteDatabase db, string collection) : base(db, collectionName: "regions")
    {
        SetCollection(collection);
    }
    
    public override RegionTitle Create(RegionTitle entity)
    {
        var now = DateTime.Now;
        entity.CreatedTime = now;
        
        Collection.Insert(entity);
        
        Collection.EnsureIndex(x => x.Ids);
        Collection.EnsureIndex(x => x.TitleId);

        return Collection.Find(o => o.TitleId == entity.TitleId).FirstOrDefault() ?? throw new InvalidOperationException();
    }

    public int InsertBulk(IEnumerable<RegionTitle> entities)
    {
        var resultCount = Collection.InsertBulk(entities);
        Collection.EnsureIndex(x => x.Ids);
        Collection.EnsureIndex(x => x.TitleId);
        return resultCount;
    }

    public RegionTitle? GetTitleById(string id)
    {
        return Collection.Find(o => o.TitleId == id)?.FirstOrDefault();
    }
    
    public RegionTitle? FindTitleByIds(string id)
    {
        return Collection.Find(o => o.Ids != null && o.Ids.Contains(id)).FirstOrDefault();
    }
    
    public IQueryable<RegionTitle> GetTitlesAsQueryable()
    {
        return Collection.AsQueryable();
    }

    public IEnumerable<RegionTitle> Find(Expression<Func<RegionTitle, bool>> predicate)
    {
        return Collection.Find(predicate);
    }
}