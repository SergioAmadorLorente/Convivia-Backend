using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convivia.Domain.Repositories
{
    public interface ITareaRepository
    {
        Task<string> AddAsync(Tarea tarea, CancellationToken ct = default);
        Task<List<string>> AddAsyncList(List<Tarea> tareas, CancellationToken ct = default);
        Task<Tarea?> GetAsync(string plantillaId, string tareaId, CancellationToken ct = default);
        Task<List<Tarea>> GetAllAsync(string plantillaId, CancellationToken ct = default);
        Task UpdateAsync(string id, Tarea tarea, CancellationToken ct = default);
        Task UpdateAsyncList(List<Tarea> tareas, Tarea tareaactualizada, CancellationToken ct = default);
        Task DeleteAsync(string id, CancellationToken ct = default);
    }
}