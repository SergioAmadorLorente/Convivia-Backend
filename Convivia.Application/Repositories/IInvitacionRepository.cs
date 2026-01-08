using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;

namespace Convivia.Application.Repositories
{
    public interface IInvitacionRepository : IRepository<Invitacion>
    {
        Task<IEnumerable<Invitacion>> GetByUsuarioInvitadoAsync(string usuarioInvitadoId, CancellationToken ct = default);
    }
}