using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Shared.Services
{
    public interface IFirebaseService
    {
        Task AddAsync<T>(string collection, string id, T entity, CancellationToken cancellationToken = default);
        Task AddAsync<T>(string collection, T entity, CancellationToken cancellationToken = default);
        Task<T?> GetAsync<T>(string collection, string id, CancellationToken cancellationToken = default) where T : class;
        Task UpdateAsync<T>(string collection, string id, T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(string collection, string id, CancellationToken cancellationToken = default);
        Task<List<T>> QueryAsync<T>(string collection, string field, object value, CancellationToken cancellationToken = default) where T : class;
        Task<List<T>> QueryMultipleConditionsAsync<T>(string collection, (string field, object val)[] conditions, CancellationToken cancellationToken = default) where T : class;
        Task<List<T>> GetAllAsync<T>(string collection, CancellationToken cancellationToken = default) where T : class;

        // Collection group helpers
        Task<List<T>> QueryCollectionGroupAsync<T>(string subcollectionName, string field, object value, CancellationToken cancellationToken = default) where T : class;
        Task<List<T>> QueryCollectionGroupAllAsync<T>(string subcollectionName, CancellationToken cancellationToken = default) where T : class;
    }
}