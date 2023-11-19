using System.Linq.Expressions;
using NsxLibraryManager.Core.Models;

namespace NsxLibraryManager.Core.Repository.Interface;

public interface IRegionRepository: IBaseRepository<RegionTitle>
{
    public IQueryable<RegionTitle> GetTitlesAsQueryable();
    public RegionTitle? FindTitleByIds(string id);
    public RegionTitle? GetTitleById(string id);
    public new RegionTitle? FindOne(Expression<Func<RegionTitle, bool>> predicate);
    public int InsertBulk(IEnumerable<RegionTitle> entities);
    public IEnumerable<RegionTitle> Find(Expression<Func<RegionTitle, bool>> predicate);
}