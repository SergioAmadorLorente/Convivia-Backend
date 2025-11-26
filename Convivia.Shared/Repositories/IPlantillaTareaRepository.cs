using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convivia.Shared.Repositories
{
    public interface IPlantillaTareaRepository<T> where T : class
    {
        Task AddAsync(T plantilla);
        Task<T?> GetAsync(string id);
        Task<List<T>> GetAllAsync();
        Task UpdateAsync(T plantilla);
        Task DeleteAsync(string id);
        Task<List<T>> QueryByFieldAsync(string field, object value);
    }
}