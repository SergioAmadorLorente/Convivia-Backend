using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convivia.Domain.Repositories
{
    public interface ITareaRepository
    {
        Task<string> AddAsync(Tarea tarea, CancellationToken ct = default);
        Task<Tarea?> GetAsync(string id, CancellationToken ct = default);
        Task<List<Tarea>> GetAllAsync(CancellationToken ct = default);
        Task UpdateAsync(string id, Tarea tarea, CancellationToken ct = default);
        Task DeleteAsync(string id, CancellationToken ct = default);
        Task<List<Tarea?>> GetAllByEspacioIdAsync(string espacioid, CancellationToken ct = default);
    }
}