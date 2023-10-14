using NsxLibraryManager.Models;

namespace NsxLibraryManager.Repository;

public interface ITitleDbVersionsRepository: IBaseRepository<GameVersions>
{
    public int InsertBulk(IEnumerable<GameVersions> entities);

    public IEnumerable<GameVersions> FindByTitleId(string titleId);
}