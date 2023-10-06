using NsxLibraryManager.Models;

namespace NsxLibraryManager.Repository;

public interface ITitleRepository : IBaseRepository<TitleDbTitle>
{
    TitleDbTitle? GetTitleById(string id);

}