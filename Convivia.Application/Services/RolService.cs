using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Microsoft.Extensions.Logging;
using Convivia.Shared.Repositories;
using Mapster;
using Convivia.Domain.Entities;

namespace Convivia.Application.Services
{
    public class RolService
    {
        private readonly IRolRepository _repo;
        private readonly ILogger<RolService> _logger;
        private static readonly string[] NombresValidos = { "Usuario", "Admin" };

        public RolService(IRolRepository repo, ILogger<RolService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> CrearAsync(CreateRolDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("Nombre requerido");

            if (!EsNombreValido(dto.Nombre))
            {
                throw new ArgumentException($"Nombre '{dto.Nombre}' no válido. Los nombres permitidos son: {string.Join(", ", NombresValidos)}");
            }

            // Crear rol con la configuración apropiada
            Rol rolDomain = new Rol();
            
            if (dto.Nombre.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                rolDomain.SetConfigurarcionAdmin();
            }
            else
            {
                rolDomain.SetConfigurarcionUsuario();
            }

            // Mapster maneja automáticamente Rol -> RolDto
            var rolDto = rolDomain.Adapt<RolDto>();

            try
            {
                return await _repo.AddAsync(rolDto, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando rol");
                throw;
            }
        }

        public async Task<RolDto?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
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

        public async Task<IEnumerable<RolDto>> ObtenerTodosAsync(CancellationToken ct = default)
        {
            try
            {
                return await _repo.GetAllAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerTodos");
                throw;
            }
        }

        public async Task<RolDto?> ObtenerPorNombreAsync(string nombre, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return null;
            
            if (!EsNombreValido(nombre))
            {
                throw new ArgumentException($"Nombre '{nombre}' no válido. Los nombres permitidos son: {string.Join(", ", NombresValidos)}");
            }

            try
            {
                return await _repo.GetByNombreAsync(nombre, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorNombre {Nombre}", nombre);
                throw;
            }
        }

        public async Task<bool> ActualizarAsync(string id, UpdateRolDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await ObtenerPorIdAsync(id, ct);
            if (existing == null) throw new KeyNotFoundException("Rol no encontrado");

            if (dto.Nombre != null && !EsNombreValido(dto.Nombre))
            {
                throw new ArgumentException($"Nombre '{dto.Nombre}' no válido. Los nombres permitidos son: {string.Join(", ", NombresValidos)}");
            }

            // Mapster maneja automáticamente el merge de UpdateRolDto -> RolDto
            dto.Adapt(existing);

            try
            {
                await _repo.UpdateAsync(id, existing, ct);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Actualizar {Id}", id);
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
                _logger.LogError(ex, "Error EliminarRol {Id}", id);
                throw;
            }
        }

        private static bool EsNombreValido(string nombre)
        {
            return !string.IsNullOrWhiteSpace(nombre) && 
                   NombresValidos.Contains(nombre, StringComparer.OrdinalIgnoreCase);
        }
    }
}
