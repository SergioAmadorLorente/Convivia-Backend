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
    }
}