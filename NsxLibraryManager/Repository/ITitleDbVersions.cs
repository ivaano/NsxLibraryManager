using NsxLibraryManager.Models;

namespace NsxLibraryManager.Repository;

public interface ITitleDbVersions: IBaseRepository<GameVersions>
{
    public int InsertBulk(IEnumerable<GameVersions> entities);
}