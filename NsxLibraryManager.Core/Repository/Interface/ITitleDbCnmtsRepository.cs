using NsxLibraryManager.Core.Models;

namespace NsxLibraryManager.Core.Repository.Interface;

public interface ITitleDbCnmtsRepository : IBaseRepository<PackagedContentMeta>
{
    public int InsertBulk(IEnumerable<PackagedContentMeta> entities);
    public IEnumerable<PackagedContentMeta> FindByOtherApplicationId(string titleId);

}