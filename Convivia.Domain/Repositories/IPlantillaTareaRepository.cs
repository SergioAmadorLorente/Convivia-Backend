using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convivia.Domain.Repositories
{
    public interface IPlantillaTareaRepository
    {
        Task<string> AddAsync(PlantillaTarea plantilla, CancellationToken ct = default);
        Task<PlantillaTarea?> GetAsync(string id, CancellationToken ct = default);
        Task<List<PlantillaTarea>> GetAllAsync(CancellationToken ct = default);
        Task UpdateAsync(string id, PlantillaTarea plantilla, CancellationToken ct = default);
        Task DeleteAsync(string id, CancellationToken ct = default);
        Task<IEnumerable<Tarea>> GetByUsuarioEspacioIdAsync(string usuarioespacioid, CancellationToken ct = default);
    }
}