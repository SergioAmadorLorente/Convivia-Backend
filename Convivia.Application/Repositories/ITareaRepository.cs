using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convivia.Application.Repositories
{
    public interface ITareaRepository : IRepository<Tarea>
    {
        Task<Tarea?> GetInstanciaAsync(string plantillaId, string tareaId, CancellationToken ct = default);
        Task<Tarea?> GetInstanciaAsync(string espacioId, string plantillaId, string tareaId, CancellationToken ct = default);
        Task<List<string>> AddAsyncList(string espacioId, List<Tarea> tareas, CancellationToken ct = default);
        Task<string> AddAsync(string espacioId, Tarea tarea, CancellationToken ct = default);
        Task UpdateAsync(string espacioId, string id, Tarea tareaActualizada, CancellationToken ct = default);
        Task UpdateAsync(string espacioId, string id, Tarea tareaActualizada, bool merge, CancellationToken ct = default);
        Task<List<Tarea>> GetByUsuarioEspacioIdAsync(string usuarioEspacioId, CancellationToken ct);

        /// <summary>
        /// Carga en paralelo todas las instancias de tareas de las plantillas indicadas para un espacio.
        /// Reduce el patrón N+1 (1 query por tarea) a M queries en paralelo (1 por plantilla).
        /// Retorna un diccionario keyed por PlantillaId con la lista de tareas de cada plantilla.
        /// </summary>
        Task<Dictionary<string, List<Tarea>>> GetAllByEspacioGroupedByPlantillaAsync(
            string espacioId,
            IEnumerable<string> plantillaIds,
            CancellationToken ct = default);
    }
}