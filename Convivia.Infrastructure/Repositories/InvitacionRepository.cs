using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Convivia.Infrastructure.Models;
using Convivia.Domain.Entities;
using Microsoft.Extensions.Logging;
using Mapster;

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
            
            // Convertir InvitacionDto → Invitacion (Domain) → FireStoreInvitacion (Firestore)
            var invitacionDomain = invitacion.Adapt<Invitacion>();
            var invitacionPersist = invitacionDomain.Adapt<FireStoreInvitacion>();

            // Si ya tiene id, lo usamos para crear el documento con ese id
            await _firebase.AddAsync(Collection, invitacionPersist.Id, invitacionPersist, ct);
            return invitacionPersist.Id;
        }

        public async Task<InvitacionDto?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                // Obtener FireStoreInvitacion de Firestore
                var invitacionPersist = await _firebase.GetAsync<FireStoreInvitacion>(Collection, id, ct);
                if (invitacionPersist == null) return null;
                
                // Convertir FireStoreInvitacion → Invitacion (Domain) → InvitacionDto
                var invitacionDomain = invitacionPersist.Adapt<Invitacion>();
                return invitacionDomain.Adapt<InvitacionDto>();
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
                // Consultar FireStoreInvitacion desde Firestore
                var list = await _firebase.QueryAsync<FireStoreInvitacion>(Collection, nameof(FireStoreInvitacion.UsuarioInvitadoId), usuarioInvitadoId, ct);
                if (list == null || !list.Any()) return new List<InvitacionDto>();
                
                // Convertir FireStoreInvitacion → Invitacion (Domain) → InvitacionDto
                return list.Select(ip => ip.Adapt<Invitacion>().Adapt<InvitacionDto>()).ToList();
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
                // Convertir InvitacionDto → Invitacion (Domain) → FireStoreInvitacion
                var invitacionDomain = invitacion.Adapt<Invitacion>();
                var invitacionPersist = invitacionDomain.Adapt<FireStoreInvitacion>();
                
                await _firebase.UpdateAsync(Collection, id, invitacionPersist, ct);
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