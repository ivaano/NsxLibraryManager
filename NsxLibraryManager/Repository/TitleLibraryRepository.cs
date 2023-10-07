using LiteDB;
using NsxLibraryManager.Models;
using NsxLibraryManager.Settings;

namespace NsxLibraryManager.Repository;

public class TitleLibraryRepository : BaseRepository<LibraryTitle>, ITitleLibraryRepository
{
    public TitleLibraryRepository(ILiteDatabase db) : base(db, collectionName: AppConstants.LibraryCollectionName)
    {
    }

}