using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Application.Repositories;
using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace Convivia.Application.Services
{
    public class InvitacionService
    {
        private readonly IInvitacionRepository _invitacionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<InvitacionService> _logger;

        public InvitacionService(IInvitacionRepository invitacionRepository, IMapper mapper, ILogger<InvitacionService> logger)
        {
            _invitacionRepository = invitacionRepository ?? throw new ArgumentNullException(nameof(invitacionRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        /// Crea una invitacion y devuelve la invitacion persistida (con Id y metadatos).
        /// </summary>
        public async Task<InvitacionDto> CrearInvitacionAsync(CreateInvitacionDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.UsuarioSolicitanteId)) throw new ArgumentException("UsuarioSolicitanteId requerido", nameof(dto.UsuarioSolicitanteId));
            if (string.IsNullOrWhiteSpace(dto.UsuarioInvitadoId)) throw new ArgumentException("UsuarioInvitadoId requerido", nameof(dto.UsuarioInvitadoId));
            if (string.IsNullOrWhiteSpace(dto.EspacioId)) throw new ArgumentException("EspacioId requerido", nameof(dto.EspacioId));

            // DTO -> Domain
            var invitacionDomain = _mapper.Map<Invitacion>(dto);

            // Persistir y obtener id
            var id = await _invitacionRepository.AddAsync(invitacionDomain, ct);

            // Recuperar entidad guardad y devolver DTO consistente 
            var createdDomain = await _invitacionRepository.GetByIdAsync(id, ct);
            if (createdDomain == null)
            {
                // Devolver DTO mínimo con id para evitar fallos en rutas 
                return new InvitacionDto { Id = id };
            }

            var createdDto = _mapper.Map<InvitacionDto>(createdDomain);
            if (string.IsNullOrWhiteSpace(createdDto.Id))
                createdDto.Id = id;

            return createdDto;
        }

        /// <summary>
        /// Obtiene una invitacion por id.
        /// </summary>
        public async Task<InvitacionDto?> ObtenerInvitacionAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            var domain = await _invitacionRepository.GetByIdAsync(id, ct);
            return domain == null ? null : _mapper.Map<InvitacionDto>(domain);
        }

        /// <summary>
        /// Lista todas las invitaciones.
        /// </summary>
        public async Task<List<InvitacionDto>> ListarTodasAsync(CancellationToken ct = default)
        {
            var list = await _invitacionRepository.GetAllAsync(ct);
            return list?.Select(f => _mapper.Map<InvitacionDto>(f)).ToList() ?? new List<InvitacionDto>();
        }


        /// <summary>
        /// Obtiene una invitacion por id de usuario invitado.
        /// </summary>
        public async Task<IEnumerable<InvitacionDto>> ObtenerPorUsuarioInvitadoAsync(string usuarioInvitadoId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioInvitadoId)) return Array.Empty<InvitacionDto>();
            try
            {
                var domains = await _invitacionRepository.GetByUsuarioInvitadoAsync(usuarioInvitadoId, ct);
                return domains == null ? Array.Empty<InvitacionDto>() : _mapper.Map<IEnumerable<InvitacionDto>>(domains);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorUsuarioInvitado {Usuario}", usuarioInvitadoId);
                throw;
            }
        }

        /// <summary>
        /// Elimina una invitaicon.
        /// </summary>
        public async Task<bool> EliminarInvitacionAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var existing = await _invitacionRepository.GetByIdAsync(id, ct);
            if (existing == null) return false;

            await _invitacionRepository.DeleteAsync(id, ct);
            return true;
        }

        public async Task ActualizarMensajeAsync(string id, CreateInvitacionDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _invitacionRepository.GetByIdAsync(id, ct);
            if (existing == null) throw new KeyNotFoundException("Invitación no encontrada");

            // Aplicar cambio de mensaje (solo este flujo)
            existing.Mensaje = dto.Mensaje ?? existing.Mensaje;

            try
            {
                await _invitacionRepository.UpdateAsync(id, existing, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ActualizarMensaje {Id}", id);
                throw;
            }
        }
    }
}
