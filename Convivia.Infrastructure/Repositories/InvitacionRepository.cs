using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.Services;
using Convivia.Infrastructure.Models;
using Convivia.Domain.Entities;
using Convivia.Application.Repositories;
using Microsoft.Extensions.Logging;
using Mapster;

namespace Convivia.Infrastructure.Repositories
{
    public class InvitacionRepository : Repository<FireStoreInvitacion>, IInvitacionRepository
    {
        private readonly ILogger<InvitacionRepository> _logger;
        private const string Collection = "invitaciones";

        public InvitacionRepository(IFirebaseService firebase, ILogger<InvitacionRepository> logger, ILoggerFactory loggerFactory)
            : base(firebase, loggerFactory.CreateLogger<Repository<FireStoreInvitacion>>(), Collection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Add: si la entidad trae Id la usamos, sino dejamos que Firestore lo genere
        public async Task<string> AddAsync(Invitacion invitacion, CancellationToken ct = default)
        {
            if (invitacion == null) throw new ArgumentNullException(nameof(invitacion));
            var persist = invitacion.Adapt<FireStoreInvitacion>();
            return await base.AddAsync(persist,persist.Id, ct);
        }

        public async Task<Invitacion?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var persist = await base.GetByIdAsync(id, ct);
            if (persist == null) return null;
            return persist.Adapt<Invitacion>();
        }

        public async Task<IEnumerable<Invitacion>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await base.GetAllAsync(ct);
            return list == null ? Array.Empty<Invitacion>() : list.Adapt<List<Invitacion>>();
        }

        public async Task<IEnumerable<Invitacion>> GetByUsuarioInvitadoAsync(string usuarioInvitadoId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioInvitadoId)) return Array.Empty<Invitacion>();

            try
            {
                var list = await _firebase.QueryAsync<FireStoreInvitacion>(Collection, nameof(FireStoreInvitacion.UsuarioInvitadoId), usuarioInvitadoId, ct);
                return list == null ? Array.Empty<Invitacion>() : list.Adapt<List<Invitacion>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByUsuarioInvitadoAsync {Usuario}", usuarioInvitadoId);
                throw;
            }
        }
        public async Task UpdateAsync(string id, Invitacion invitacion, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (invitacion == null) throw new ArgumentNullException(nameof(invitacion));

            var persist = invitacion.Adapt<FireStoreInvitacion>();
            await base.UpdateAsync(id, persist, ct);
        }

        public async Task UpdateAsync(string id, Invitacion invitacion, bool merge, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (invitacion == null) throw new ArgumentNullException(nameof(invitacion));

            var persist = invitacion.Adapt<FireStoreInvitacion>();
            await base.UpdateAsync(id, persist, merge, ct);
        }

        public async Task UpdateAsync(string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default)
        {
            await base.UpdateAsync(id, updates, useSetMerge, ct);
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            await base.DeleteAsync(id, ct);
        }
    }
}