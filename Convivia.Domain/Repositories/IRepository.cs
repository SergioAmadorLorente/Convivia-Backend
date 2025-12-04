using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Convivia.Infrastructure.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<T?> AddAsync(T entity, CancellationToken ct = default);
        Task<T?> UpdateAsync(string id, T entity, CancellationToken ct = default);
        Task DeleteAsync(string id, CancellationToken ct = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    }
}