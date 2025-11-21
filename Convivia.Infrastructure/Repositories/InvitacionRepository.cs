using Convivia.Application.DTOs;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.Repositories;

namespace Convivia.Infrastructure.Repositories
{
    public class InvitacionRepository : IInvitacionRepository
    {
        private readonly IFirebaseService _firebase;
        private readonly ILogger<InvitacionRepository> _logger;
        private const string Collection = "invitaciones";

        public InvitacionRepository(IFirebaseService firebase, ILogger<InvitacionRepository> logger)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(InvitacionDto invitacion, CancellationToken ct = default)
        {
            if (invitacion == null) throw new ArgumentNullException(nameof(invitacion));
            if (string.IsNullOrWhiteSpace(invitacion.Id))
            {
                // Si no tiene id, pedimos a Firestore que genere una id y la devolvemos
                var generatedId = await _firebase.AddAsync(Collection, invitacion, ct);
                return generatedId;
            }

            // Si ya tiene id, lo usamos para crear el documento con ese id
            await _firebase.AddAsync(Collection, invitacion.Id, invitacion, ct);
            return invitacion.Id;
        }

        public async Task<InvitacionDto?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                return await _firebase.GetAsync<InvitacionDto>(Collection, id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByIdAsync {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<InvitacionDto>> GetByUsuarioInvitadoAsync(string usuarioInvitadoId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioInvitadoId)) return Array.Empty<InvitacionDto>();
            try
            {
                var list = await _firebase.QueryAsync<InvitacionDto>(Collection, nameof(InvitacionDto.UsuarioInvitadoId), usuarioInvitadoId, ct);
                return list ?? new List<InvitacionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByUsuarioInvitadoAsync {Usuario}", usuarioInvitadoId);
                throw;
            }
        }

        public async Task UpdateAsync(string id, InvitacionDto invitacion, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));
            if (invitacion == null) throw new ArgumentNullException(nameof(invitacion));

            try
            {
                await _firebase.UpdateAsync(Collection, id, invitacion, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error UpdateAsync {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));
            try
            {
                await _firebase.DeleteAsync(Collection, id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error DeleteAsync {Id}", id);
                throw;
            }
        }
    }
}