using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convivia.Application.Repositories
{
    public interface ITareaRepository : IRepository<Tarea>
    {
        Task<List<Tarea>> GetAllByEspacioIdAsync(string espacioid, CancellationToken ct = default);
    }
}