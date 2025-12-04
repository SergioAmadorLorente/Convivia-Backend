using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Microsoft.Extensions.Logging;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Mapster;

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

            // Usar Mapster para mapear CreateEspacioDto -> EspacioDto
            var espacio = dto.Adapt<EspacioDto>();
            espacio.Id = Guid.NewGuid().ToString("N");

            try
            {
                return await _repo.AddAsync(espacio, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando espacio");
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

        public async Task<IEnumerable<EspacioDto>> ObtenerPorDireccionAsync(string direccion, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(direccion)) return Enumerable.Empty<EspacioDto>();
            try
            {
                return await _repo.GetByDireccionAsync(direccion, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorDireccion {Direccion}", direccion);
                throw;
            }
        }

        public async Task ActualizarAsync(string id, CreateEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido");
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            try
            {
                var espacioExistente = await _repo.GetByIdAsync(id, ct);
                if (espacioExistente == null) throw new KeyNotFoundException($"Espacio con Id {id} no encontrado");

                espacioExistente.Nombre = dto.Nombre ?? espacioExistente.Nombre;
                espacioExistente.Direccion = dto.Direccion ?? espacioExistente.Direccion;

                await _repo.UpdateAsync(id, espacioExistente, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando espacio {Id}", id);
                throw;
            }
        }

        public async Task EliminarAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido");
            try
            {
                await _repo.DeleteAsync(id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando espacio {Id}", id);
                throw;
            }
        }

        public async Task<bool> ParcialActualizarAsync(string id, UpdateEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido");
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            try
            {
                var espacio = await _repo.GetByIdAsync(id, ct);
                if (espacio == null) return false;

                var changed = false;

                if (!string.IsNullOrWhiteSpace(dto.Nombre) && dto.Nombre != espacio.Nombre)
                {
                    espacio.Nombre = dto.Nombre;
                    changed = true;
                }

                if (dto.Direccion != null && dto.Direccion != espacio.Direccion)
                {
                    espacio.Direccion = dto.Direccion;
                    changed = true;
                }

                if (!changed) return true;

                await _repo.UpdateAsync(id, espacio, ct);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parcial actualizando espacio {Id}", id);
                throw;
            }
        }
    }
}