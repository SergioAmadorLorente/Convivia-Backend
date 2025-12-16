using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IPermisoRepository _permisoRepo;
        private readonly ILogger<RolService> _logger;

        public RolService(IRolRepository repo, IPermisoRepository permisoRepo, ILogger<RolService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _permisoRepo = permisoRepo ?? throw new ArgumentNullException(nameof(permisoRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> CrearAsync(CreateRolDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Crear rol con la configuración apropiada
            Rol rolDomain = new Rol();
            
            if (dto.Nombre == TipoRol.Admin)
            {
                rolDomain.SetConfigurarcionAdmin();
            }
            else
            {
                rolDomain.SetConfigurarcionUsuario();
            }

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

        public async Task<RolDto?> ObtenerPorNombreAsync(TipoRol nombre, CancellationToken ct = default)
        {
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
                // Verificar que el rol existe
                var rol = await ObtenerPorIdAsync(id, ct);
                if (rol == null)
                {
                    _logger.LogWarning("Intento de eliminar rol inexistente {Id}", id);
                    return false;
                }

                // Verificar si hay permisos asociados a este rol
                var permisosAsociados = await _permisoRepo.GetByRolAsync(rol.Nombre, ct);
                if (permisosAsociados != null && permisosAsociados.Any())
                {
                    var cantidadPermisos = permisosAsociados.Count();
                    _logger.LogWarning("No se puede eliminar el rol {Id} porque tiene {Cantidad} permisos/usuarios asociados", id, cantidadPermisos);
                    throw new InvalidOperationException($"No se puede eliminar el rol porque tiene {cantidadPermisos} usuarios asignados. Primero debes reasignar o eliminar los usuarios asociados.");
                }

                // Si no hay permisos asociados, proceder con la eliminación
                await _repo.DeleteAsync(id, ct);
                _logger.LogInformation("Rol {Id} ({Nombre}) eliminado exitosamente", id, rol.Nombre);
                return true;
            }
            catch (InvalidOperationException)
            {
                // Re-lanzar la excepción de validación de negocio
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error EliminarRol {Id}", id);
                throw;
            }
        }
    }
}
