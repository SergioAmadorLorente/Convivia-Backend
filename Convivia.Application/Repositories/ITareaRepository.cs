using Convivia.Application.Repositories;
using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Domain.Repositories
{
    public interface ITareaRepository : IRepository<Tarea>
    {
        Task<List<string>> AddAsyncList(List<Tarea> tareas, CancellationToken ct = default);
        Task<Tarea?> GetAsync(string plantillaId, string tareaId, CancellationToken ct = default);
        Task UpdateAsync(string id, Tarea tarea, CancellationToken ct = default);
        Task UpdateAsync(string id, Tarea tarea, bool merge, CancellationToken ct = default);
        Task UpdateAsync(string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default);
    }
}