using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Microsoft.Extensions.Logging;
using Convivia.Shared.Repositories;

namespace Convivia.Application.Services
{
    public class EspacioService
    {
        private readonly IEspacioRepository _repo;
        private readonly ILogger<EspacioService> _logger;

        public EspacioService(IEspacioRepository repo, ILogger<EspacioService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> CrearAsync(CreateEspacioDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("Nombre requerido");
          

            var espacio = new EspacioDto
            {
                Id = Guid.NewGuid().ToString("N"),
                Nombre = dto.Nombre,
                Direccion = dto?.Direccion
            };

            try
            {
                return await _repo.AddAsync(espacio, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando espacio.");
                throw;
            }
        }

        public async Task<EspacioDto?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
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

        public async Task<IEnumerable<EspacioDto>> ObtenerPorDireccionAsync(string direccio, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(direccio)) return Array.Empty<EspacioDto>();
            try
            {
                return await _repo.GetByDireccionAsync(direccio, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorDireccion {Espacio}", direccio);
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

        public async Task ActualizarDireccionAsync(string id, CreateEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await ObtenerPorIdAsync(id, ct);
            if (existing == null) throw new KeyNotFoundException("Espacio no encontrado");

            existing.Direccion = dto.Direccion ?? existing.Direccion;

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