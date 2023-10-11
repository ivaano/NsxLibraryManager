using System.Linq.Expressions;
using NsxLibraryManager.Models;

namespace NsxLibraryManager.Repository;

public interface IRegionRepository: IBaseRepository<RegionTitle>
{
    public RegionTitle? FindTitleByIds(string id);
    public RegionTitle? GetTitleById(string id);
    public RegionTitle? FindOne(Expression<Func<RegionTitle, bool>> predicate);
    public int InsertBulk(IEnumerable<RegionTitle> entities);
    public IEnumerable<RegionTitle> Find(Expression<Func<RegionTitle, bool>> predicate);
}