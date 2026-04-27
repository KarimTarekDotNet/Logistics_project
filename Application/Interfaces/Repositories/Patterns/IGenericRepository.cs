using System.Linq.Expressions;

namespace Application.Interfaces.Repositories.Patterns
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id, Func<IQueryable<T>, IQueryable<T>>? include = null);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IQueryable<T>>? include = null);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}