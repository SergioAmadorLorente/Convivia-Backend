using Convivia.Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Infrastructure.Repositories
{
    public interface IInvitacionRepository
    {
        Task<InvitacionDto?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<IEnumerable<InvitacionDto>> GetByUsuarioInvitadoAsync(string usuarioInvitadoId, CancellationToken ct = default);
        Task<string> AddAsync(InvitacionDto invitacion, CancellationToken ct = default);
        Task UpdateAsync(string id, InvitacionDto invitacion, CancellationToken ct = default);
        Task DeleteAsync(string id, CancellationToken ct = default);
    }
}