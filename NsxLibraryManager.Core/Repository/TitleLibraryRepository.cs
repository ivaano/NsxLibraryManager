using LiteDB;
using LiteDB.Queryable;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Repository.Interface;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Core.Repository;

public class TitleLibraryRepository : BaseRepository<LibraryTitle>, ITitleLibraryRepository
{
    public TitleLibraryRepository(ILiteDatabase db) : base(db, collectionName: AppConstants.LibraryCollectionName)
    {
    }

    public IQueryable<LibraryTitle> GetTitlesAsQueryable()
    {
        return Collection.AsQueryable();
    }
}