using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Application.Repositories
{
    public interface IFacturaRepository
    {
        Task<string> AddAsync(string espacioId, Factura factura, CancellationToken ct = default);
        Task<Factura?> GetByIdAsync(string espacioId, string id, CancellationToken ct = default);
        Task<IEnumerable<Factura>> GetAllAsync(string espacioId, CancellationToken ct = default);
        Task UpdateAsync(string espacioId, string id, Factura factura, CancellationToken ct = default);
        Task UpdateAsync(string espacioId, string id, Factura factura, bool merge, CancellationToken ct = default);
        Task UpdateAsync(string espacioId, string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default);
        Task DeleteAsync(string espacioId, string id, CancellationToken ct = default);
        Task<IEnumerable<Factura>> QueryByFieldAsync(string espacioId, string field, object value, CancellationToken ct = default);
        Task<bool> ExistsByUsuarioEspacioIdAsync(string usuarioEspacioId, CancellationToken ct = default);
        
        Task<byte[]?> GetImagenAsync(string espacioId, string id, CancellationToken ct = default);
        Task UpdateImagenAsync(string espacioId, string id, byte[] imagen, CancellationToken ct = default);
        Task DeleteImagenAsync(string espacioId, string id, CancellationToken ct = default);
        
        Task<IEnumerable<Factura>> GetByCreadorAsync(string espacioId, string creadorId, CancellationToken ct = default);
        Task<IEnumerable<Factura>> GetByDeudorAsync(string espacioId, string deudorId, CancellationToken ct = default);
    }
}
