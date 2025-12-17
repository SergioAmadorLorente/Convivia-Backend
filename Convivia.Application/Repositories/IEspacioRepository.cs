using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;

namespace Convivia.Application.Repositories
{
    public interface IEspacioRepository : IRepository<Espacio>
    {
        Task<IEnumerable<Espacio>> GetByDireccionAsync(string Direccion, CancellationToken ct = default);
    }
}