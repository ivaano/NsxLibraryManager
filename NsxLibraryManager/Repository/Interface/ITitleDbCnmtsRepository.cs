using NsxLibraryManager.Models;

namespace NsxLibraryManager.Repository;

public interface ITitleDbCnmtsRepository : IBaseRepository<PackagedContentMeta>
{
    public int InsertBulk(IEnumerable<PackagedContentMeta> entities);
    public IEnumerable<PackagedContentMeta> FindByOtherApplicationId(string titleId);

}