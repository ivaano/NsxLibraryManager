using NsxLibraryManager.Core.Models;

namespace NsxLibraryManager.Core.Repository.Interface;

public interface ITitleDbVersionsRepository: IBaseRepository<GameVersions>
{
    public int InsertBulk(IEnumerable<GameVersions> entities);

    public IEnumerable<GameVersions> FindByTitleId(string titleId);
}