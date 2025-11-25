using System;
using Mapster;
using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;
using Microsoft.Extensions.Logging;
namespace Convivia.Application.Services
{
    public class SalaService
    {
        private readonly ISalaRepository _repo;
        private readonly ILogger<SalaService> _logger;

        public SalaService(ISalaRepository repo, ILogger<SalaService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> CrearAsync(CreateSalaDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("Nombre requerido");

            // Usar Mapster para mapear CreateSalaDto -> SalaDto
            var sala = dto.Adapt<SalaDto>();
            sala.Id = Guid.NewGuid().ToString("N");
            sala.Descripcion = sala.Descripcion ?? string.Empty;

            try
            {
                return await _repo.AddAsync(sala, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando sala");
                throw;
            }
        }

        public async Task<SalaDto?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
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

        public async Task<IEnumerable<SalaDto>> ObtenerPorEspacioAsync(string idEspacio, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(idEspacio)) return Enumerable.Empty<SalaDto>();
            try
            {
                return await _repo.GetByEspacioIdAsync(idEspacio, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorEspacio {IdEspacio}", idEspacio);
                throw;
            }
        }

        public async Task ActualizarAsync(string id, CreateSalaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido");
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            try
            {
                var salaExistente = await _repo.GetByIdAsync(id, ct);
                if (salaExistente == null) throw new KeyNotFoundException($"Sala con Id {id} no encontrada");
                salaExistente.Nombre = dto.Nombre ?? salaExistente.Nombre;
                salaExistente.Descripcion = dto.Descripcion ?? salaExistente.Descripcion;
                await _repo.UpdateAsync(id, salaExistente, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando sala {Id}", id);
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
                _logger.LogError(ex, "Error eliminando sala {Id}", id);
                throw;
            }
        }

        public async Task<bool> ParcialActualizarAsync(string id, UpdateSalaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido");
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Try que obtiene la sala, verifica cambios y actualiza solo si hay cambios.
            try
            {
                var sala = await _repo.GetByIdAsync(id, ct);
                if (sala == null) return false;

                var changed = false;

                if (!string.IsNullOrWhiteSpace(dto.Nombre) && dto.Nombre != sala.Nombre)
                {
                    sala.Nombre = dto.Nombre;
                    changed = true;
                }

                if (!string.IsNullOrWhiteSpace(dto.Descripcion) && dto.Descripcion != sala.Descripcion)
                {
                    sala.Descripcion = dto.Descripcion;
                    changed = true;
                }

                if (!string.IsNullOrWhiteSpace(dto.IdEspacio) && dto.IdEspacio != sala.IdEspacio)
                {
                    sala.IdEspacio = dto.IdEspacio;
                    changed = true;
                }

                if (!changed) return true; // nada que actualizar

                await _repo.UpdateAsync(id, sala, ct);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parcial actualizando sala {Id}", id);
                throw;
            }
        }
    }
}
