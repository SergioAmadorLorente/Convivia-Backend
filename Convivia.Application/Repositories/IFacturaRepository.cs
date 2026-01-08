using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Application.Repositories
{
    public interface IFacturaRepository : IRepository<Factura>
    {
        Task<IEnumerable<Factura>> QueryByFieldAsync(string field, object value, CancellationToken ct = default);
        Task<bool> ExistsByUsuarioEspacioIdAsync(string espacioId, CancellationToken ct = default);
    }
}
