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
    public class PermisoService
    {
        private readonly IPermisoRepository _repo;
        private readonly ILogger<PermisoService> _logger;

        public PermisoService(IPermisoRepository repo, ILogger<PermisoService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> CrearAsync(CreatePermisoDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Mapster convierte CreatePermisoDto -> Permiso
            // RolTypeConverter maneja TipoRol -> Rol automáticamente
            var permisoDomain = dto.Adapt<Permiso>();

            // Mapster convierte Permiso -> PermisoDto
            var permisoDto = permisoDomain.Adapt<PermisoDto>();

            try
            {
                return await _repo.AddAsync(permisoDto, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando permiso");
                throw;
            }
        }

        public async Task<PermisoDto?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
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

        public async Task<IEnumerable<PermisoDto>> ObtenerPorRolAsync(TipoRol rol, CancellationToken ct = default)
        {
            try
            {
                return await _repo.GetByRolAsync(rol, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorRol {Rol}", rol);
                throw;
            }
        }

        public async Task<IEnumerable<PermisoDto>> ObtenerTodosAsync(CancellationToken ct = default)
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

        public async Task<bool> ActualizarAsync(string id, UpdatePermisoDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await ObtenerPorIdAsync(id, ct);
            if (existing == null) throw new KeyNotFoundException("Permiso no encontrado");

            // Si se cambia el rol, actualizar todo según la configuración del rol
            if (dto.Rol.HasValue && dto.Rol.Value != existing.Rol)
            {
                var rolDomain = new Rol();
                if (dto.Rol.Value == TipoRol.Admin)
                {
                    rolDomain.SetConfigurarcionAdmin();
                }
                else
                {
                    rolDomain.SetConfigurarcionUsuario();
                }
                
                existing.Rol = dto.Rol.Value;
                existing.CrearTarea = rolDomain.CrearTarea;
                existing.EliminarTarea = rolDomain.EliminarTarea;
                existing.EditarTarea = rolDomain.EditarTarea;
                existing.AñadirUsuario = rolDomain.AñadirUsuario;
                existing.EliminarUsuario = rolDomain.EliminarUsuario;
                existing.AsignarTarea = rolDomain.AsignarTarea;
                existing.AsignarseTarea = rolDomain.AsignarseTarea;
            }
            else
            {
                // Si no se cambia el rol, permitir actualizar permisos individuales
                if (dto.CrearTarea.HasValue) existing.CrearTarea = dto.CrearTarea.Value;
                if (dto.EliminarTarea.HasValue) existing.EliminarTarea = dto.EliminarTarea.Value;
                if (dto.EditarTarea.HasValue) existing.EditarTarea = dto.EditarTarea.Value;
                if (dto.AñadirUsuario.HasValue) existing.AñadirUsuario = dto.AñadirUsuario.Value;
                if (dto.EliminarUsuario.HasValue) existing.EliminarUsuario = dto.EliminarUsuario.Value;
                if (dto.AsignarTarea.HasValue) existing.AsignarTarea = dto.AsignarTarea.Value;
                if (dto.AsignarseTarea.HasValue) existing.AsignarseTarea = dto.AsignarseTarea.Value;
            }

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
                // Verificar que el permiso existe
                var permiso = await ObtenerPorIdAsync(id, ct);
                if (permiso == null)
                {
                    _logger.LogWarning("Intento de eliminar permiso inexistente {Id}", id);
                    return false;
                }

                // Proceder con la eliminación
                await _repo.DeleteAsync(id, ct);
                _logger.LogInformation("Permiso {Id} eliminado exitosamente", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error EliminarPermiso {Id}", id);
                throw;
            }
        }
    }
}
