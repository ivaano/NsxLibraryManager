using NsxLibraryManager.Core.Models;

namespace NsxLibraryManager.Core.Repository.Interface;

public interface ITitleRepository : IBaseRepository<TitleDbTitle>
{
    TitleDbTitle? GetTitleById(string id);

}