using Convivia.Application.Common;
using Convivia.Application.Repositories;
using Convivia.Domain.Entities;
using Convivia.Shared;
using Convivia.Shared.Correlation;
using Convivia.Shared.DTOs;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Application.Services
{
    public class InvitacionService : ServiceBase
    {
        private readonly IInvitacionRepository _invitacionRepository;
        private readonly IMapper _mapper;

        public InvitacionService(
            IInvitacionRepository invitacionRepository,
            IMapper mapper,
            ILogger<InvitacionService> logger,
            ICorrelationProvider correlation)
            : base(logger, correlation)
        {
            _invitacionRepository = invitacionRepository ?? throw new ArgumentNullException(nameof(invitacionRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<InvitacionDto> CrearInvitacionAsync(CreateInvitacionDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.UsuarioSolicitanteId)) throw new ArgumentException("UsuarioSolicitanteId requerido", nameof(dto.UsuarioSolicitanteId));
            if (string.IsNullOrWhiteSpace(dto.UsuarioInvitadoId)) throw new ArgumentException("UsuarioInvitadoId requerido", nameof(dto.UsuarioInvitadoId));
            if (string.IsNullOrWhiteSpace(dto.EspacioId)) throw new ArgumentException("EspacioId requerido", nameof(dto.EspacioId));

            var invitacionDomain = _mapper.Map<Invitacion>(dto);

            using (BeginCorrelationScope())
            {
                var id = await _invitacionRepository.AddAsync(invitacionDomain, ct);
                var createdDomain = await _invitacionRepository.GetByIdAsync(id, ct);
                if (createdDomain == null) return new InvitacionDto { Id = id };

                var createdDto = _mapper.Map<InvitacionDto>(createdDomain);
                if (string.IsNullOrWhiteSpace(createdDto.Id)) createdDto.Id = id;
                return createdDto;
            }
        }

        public async Task<InvitacionDto?> ObtenerInvitacionAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            var domain = await _invitacionRepository.GetByIdAsync(id, ct);
            return domain == null ? null : _mapper.Map<InvitacionDto>(domain);
        }

        public async Task<List<InvitacionDto>> ListarTodasAsync(CancellationToken ct = default)
        {
            var list = await _invitacionRepository.GetAllAsync(ct);
            return list?.Select(f => _mapper.Map<InvitacionDto>(f)).ToList() ?? new List<InvitacionDto>();
        }

        public async Task<IEnumerable<InvitacionDto>> ObtenerPorUsuarioInvitadoAsync(string usuarioInvitadoId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioInvitadoId)) return Array.Empty<InvitacionDto>();
            try
            {
                using (BeginCorrelationScope())
                {
                    var domains = await _invitacionRepository.GetByUsuarioInvitadoAsync(usuarioInvitadoId, ct);
                    return domains == null ? Array.Empty<InvitacionDto>() : _mapper.Map<IEnumerable<InvitacionDto>>(domains);
                }
            }
            catch (Exception ex)
            {
                using (BeginCorrelationScope())
                {
                    _logger.LogError(ex, "Error ObtenerPorUsuarioInvitado {Usuario}", usuarioInvitadoId);
                }
                throw;
            }
        }

        public async Task<bool> EliminarInvitacionAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var existing = await _invitacionRepository.GetByIdAsync(id, ct);
            if (existing == null) return false;

            using (BeginCorrelationScope())
            {
                await _invitacionRepository.DeleteAsync(id, ct);
            }
            return true;
        }

        public async Task ActualizarMensajeAsync(string id, CreateInvitacionDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _invitacionRepository.GetByIdAsync(id, ct);
            if (existing == null) throw new KeyNotFoundException("Invitación no encontrada");

            existing.Mensaje = dto.Mensaje ?? existing.Mensaje;

            try
            {
                using (BeginCorrelationScope())
                {
                    await _invitacionRepository.UpdateAsync(id, existing, ct);
                }
            }
            catch (Exception ex)
            {
                using (BeginCorrelationScope())
                {
                    _logger.LogError(ex, "Error ActualizarMensaje {Id}", id);
                }
                throw;
            }
        }
    }
}
