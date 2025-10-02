using System.Collections.Generic;

using System.Threading.Tasks;

namespace AuthApiDemo.Services

{

    public interface IFirebaseService

    {

        Task AddAsync<T>(string collection, string id, T entity);

        Task<T?> GetAsync<T>(string collection, string id) where T : class;

        Task UpdateAsync<T>(string collection, string id, T entity);

        Task DeleteAsync(string collection, string id);

        Task<List<T>> QueryAsync<T>(string collection, string field, object value) where T : class;

        Task<List<T>> QueryMultipleConditionsAsync<T>(

            string collection,

            (string field, object val)[] conditions

        ) where T : class;

    }

}

