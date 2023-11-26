using System.Linq.Expressions;
using LiteDB;
using NsxLibraryManager.Core.Repository.Interface;

namespace NsxLibraryManager.Core.Repository;

public abstract class BaseRepository<T> : IBaseRepository<T>
{
    private ILiteDatabase Db { get; }
    protected ILiteCollection<T> Collection { get; set; }

    protected BaseRepository(ILiteDatabase db)
    {
        Db = db;
        Collection = db.GetCollection<T>();
    }
    
    protected BaseRepository(ILiteDatabase db, string collectionName)
    {
        Db = db;
        Collection = db.GetCollection<T>(collectionName);
    }

    protected virtual void SetCollection(string collectionName)
    {
        Collection = Db.GetCollection<T>(collectionName);
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
        return Db.DropCollection(collectionName);
    }
    
    public virtual T? FindOne(Expression<Func<T, bool>> predicate)
    {
        return Collection.FindOne(predicate);
    }
}

