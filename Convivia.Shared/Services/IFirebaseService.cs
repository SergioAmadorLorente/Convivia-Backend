using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Shared.Services
{
    public interface IFirebaseService
    {
        Task AddAsync<T>(string collection, string id, T entity, CancellationToken cancellationToken = default);
        Task<string> AddAsync<T>(string collection, T entity, CancellationToken cancellationToken = default);
        Task<T?> GetAsync<T>(string collection, string id, CancellationToken cancellationToken = default) where T : class;
        Task UpdateAsync<T>(string collection, string id, T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync<T>(string collection, string id, T entity, bool merge, CancellationToken cancellationToken = default);
        Task UpdateAsync(string collection, string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken cancellationToken = default);
        Task DeleteAsync(string collection, string id, CancellationToken cancellationToken = default);
        Task<List<T>> QueryAsync<T>(string collection, string field, object value, CancellationToken cancellationToken = default) where T : class;
        Task<List<T>> QueryMultipleConditionsAsync<T>(string collection, (string field, object val)[] conditions, CancellationToken cancellationToken = default) where T : class;
        Task<List<T>> GetAllAsync<T>(string collection, CancellationToken cancellationToken = default) where T : class;

        Task<int> CountAsync(string collection, CancellationToken cancellationToken = default);
    }
}