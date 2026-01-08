using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convivia.Application.Repositories
{
    public interface IPlantillaTareaRepository : IRepository<PlantillaTarea>
    {
        Task<PlantillaTarea?> GetByEspacioAndIdAsync(string espacioid, string id, CancellationToken ct = default);
        Task<IEnumerable<Tarea>> GetByUsuarioEspacioIdAsync(string usuarioespacioid, CancellationToken ct = default);
    }
}