namespace Calc_Service_API.Data
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T Get(Guid id);
        T Add(T entity);
        void Remove(Guid id);
    }
}
