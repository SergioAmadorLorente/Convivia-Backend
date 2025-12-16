using Convivia.Shared.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Domain.Repositories
{
    public interface IFacturaRepository
    {
        Task<string> AddAsync(CreateFacturaDto factura, CancellationToken ct = default);
        Task<FacturaDto?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<IEnumerable<FacturaDto>> GetAllAsync(CancellationToken ct = default);
        Task UpdateAsync(string id, UpdateFacturaDto factura, CancellationToken ct = default);
        Task DeleteAsync(string id, CancellationToken ct = default);
        Task<IEnumerable<FacturaDto>> QueryByFieldAsync(string field, object value, CancellationToken ct = default);
    }
}
