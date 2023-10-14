using NsxLibraryManager.Models;

namespace NsxLibraryManager.Repository;

public interface ITitleLibraryRepository : IBaseRepository<LibraryTitle>
{
    IQueryable<LibraryTitle> GetTitlesAsQueryable();
}