using LiteDB;
using NsxLibraryManager.Models;
using NsxLibraryManager.Settings;

namespace NsxLibraryManager.Repository;

public class GameRepository : BaseRepository<Game>, IGameRepository
{
    public GameRepository(ILiteDatabase db) : base(db, collectionName: AppConstants.GameCollectionName)
    {
    }

    public GameRepository(ILiteDatabase db, string collectionName) : base(db, collectionName)
    {
    }
}