using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convivia.Application.Repositories
{
    public interface IPlantillaTareaRepository : IRepository<PlantillaTarea>
    {
        Task<PlantillaTarea?> GetByEspacioAndIdAsync(string espacioid, string id, CancellationToken ct = default);
        Task<IEnumerable<PlantillaTarea>> GetAllByEspacioAsync(string espacioId, CancellationToken ct = default);
        Task DeleteAsync(string espacioId, string id, CancellationToken ct = default);
        Task UpdateAsync(string espacioId, string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default);
    }
}