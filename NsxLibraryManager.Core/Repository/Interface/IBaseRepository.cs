using System.Linq.Expressions;

namespace NsxLibraryManager.Core.Repository.Interface;

public interface IBaseRepository<T>
{
    T Create(T data);
    IEnumerable<T> All();
    T FindById(int id);
    void Update(T entity);
    bool Delete(int id);
    
    bool Drop(string collectionName);
    T? FindOne(Expression<Func<T, bool>> predicate);
}