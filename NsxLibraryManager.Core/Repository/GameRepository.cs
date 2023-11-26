using LiteDB;
using NsxLibraryManager.Core.Models.Dto;
using NsxLibraryManager.Core.Repository.Interface;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Core.Repository;

public class GameRepository : BaseRepository<Game>, IGameRepository
{
    public GameRepository(ILiteDatabase db) : base(db, collectionName: AppConstants.GameCollectionName)
    {
    }

    public GameRepository(ILiteDatabase db, string collectionName) : base(db, collectionName)
    {
    }
}