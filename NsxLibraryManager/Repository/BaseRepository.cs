using LiteDB;

namespace NsxLibraryManager.Repository;

public abstract class BaseRepository<T> : IBaseRepository<T>
{
    public ILiteDatabase DB { get; }
    public ILiteCollection<T> Collection { get; set; }

    protected BaseRepository(ILiteDatabase db)
    {
        DB = db;
        Collection = db.GetCollection<T>();
    }
    
    protected BaseRepository(ILiteDatabase db, string collectionName)
    {
        DB = db;
        Collection = db.GetCollection<T>(collectionName);
    }
    
    public virtual void SetCollection(string collectionName)
    {
        Collection = DB.GetCollection<T>(collectionName);
    }

    public virtual T Create(T entity)
    {
        var newId = Collection.Insert(entity);
        return Collection.FindById(newId);
    }

    public virtual IEnumerable<T> All()
    {
        return Collection.FindAll();
    }

    public virtual T FindById(int id)
    {
        return Collection.FindById(id);
    }

    public virtual void Update(T entity)
    {
        Collection.Upsert(entity);
    }

    public virtual bool Delete(int id)
    {
        return Collection.Delete(id);
    }

    public virtual bool Drop(string collectionName)
    {
        return DB.DropCollection(collectionName);
    }
}

