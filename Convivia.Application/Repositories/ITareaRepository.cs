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
        Task<Tarea?> GetInstanciaAsync(string plantillaId, string tareaId, CancellationToken ct = default);
        Task<List<string>> AddAsyncList(List<Tarea> tareas, CancellationToken ct = default);
    }
}