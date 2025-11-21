using Convivia.Application.DTOs;
using Convivia.Infrastructure.Repositories;
using Convivia.Shared.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Application.Services
{
    public class InvitacionService
    {
        private readonly IInvitacionRepository _repo;
        private readonly ILogger<InvitacionService> _logger;

        public InvitacionService(IInvitacionRepository repo, ILogger<InvitacionService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> CrearAsync(CreateInvitacionDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.UsuarioSolicitanteId)) throw new ArgumentException("UsuarioSolicitanteId requerido");
            if (string.IsNullOrWhiteSpace(dto.UsuarioInvitadoId)) throw new ArgumentException("UsuarioInvitadoId requerido");
            if (string.IsNullOrWhiteSpace(dto.EspacioId)) throw new ArgumentException("EspacioId requerido");

            var invitacion = new InvitacionDto
            {
                Id = Guid.NewGuid().ToString("N"),
                UsuarioSolicitanteId = dto.UsuarioSolicitanteId,
                UsuarioInvitadoId = dto.UsuarioInvitadoId,
                EspacioId = dto.EspacioId,
                Mensaje = dto.Mensaje ?? string.Empty,
                Fecha = DateTime.UtcNow,
                Estado = "pendiente"
            };

            try
            {
                return await _repo.AddAsync(invitacion, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando invitación");
                throw;
            }
        }

        public async Task<InvitacionDto?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                return await _repo.GetByIdAsync(id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorId {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<InvitacionDto>> ObtenerPorUsuarioInvitadoAsync(string usuarioInvitadoId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioInvitadoId)) return Array.Empty<InvitacionDto>();
            try
            {
                return await _repo.GetByUsuarioInvitadoAsync(usuarioInvitadoId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorUsuarioInvitado {Usuario}", usuarioInvitadoId);
                throw;
            }
        }

        public async Task<bool> EliminarAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            try
            {
                await _repo.DeleteAsync(id, ct);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error EliminarInvitacion {Id}", id);
                throw;
            }
        }

        public async Task ActualizarMensajeAsync(string id, CreateInvitacionDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await ObtenerPorIdAsync(id, ct);
            if (existing == null) throw new KeyNotFoundException("Invitación no encontrada");

            existing.Mensaje = dto.Mensaje ?? existing.Mensaje;

            try
            {
                await _repo.UpdateAsync(id, existing, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ActualizarMensaje {Id}", id);
                throw;
            }
        }
    }
}