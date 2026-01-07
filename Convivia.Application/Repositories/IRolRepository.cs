using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Application.Repositories
{
    public interface IRolRepository : IRepository<Rol>
    {
        Task<Rol?> GetByNombreAsync(TipoRol nombre, CancellationToken ct = default);
    }
}
