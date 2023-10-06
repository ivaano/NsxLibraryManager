using LiteDB;
using NsxLibraryManager.Models;

namespace NsxLibraryManager.Repository;

public class TitleLibraryRepository : BaseRepository<LibraryTitle>, ITitleLibraryRepository
{
    public TitleLibraryRepository(ILiteDatabase db) : base(db, collectionName: "library")
    {
    }

}