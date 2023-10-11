namespace NsxLibraryManager.Repository;

public interface IBaseRepository<T>
{
    T Create(T data);
    IEnumerable<T> All();
    T FindById(int id);
    void Update(T entity);
    bool Delete(int id);
    
    bool Drop(string collectionName);
}