using NsxLibraryManager.Core.Models;

namespace NsxLibraryManager.Core.Repository.Interface;

public interface ITitleLibraryRepository : IBaseRepository<LibraryTitle>
{
    IQueryable<LibraryTitle> GetTitlesAsQueryable();
    bool DeleteTitle(string titleId);
}