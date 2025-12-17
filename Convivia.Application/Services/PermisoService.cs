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
            if (string.IsNullOrWhiteSpace(dto.Rol)) throw new ArgumentException("Rol requerido");

            // Validar que el rol sea válido
            if (!Permiso.EsRolValido(dto.Rol))
            {
                throw new ArgumentException($"Rol '{dto.Rol}' no válido. Los roles permitidos son: {string.Join(", ", Permiso.RolesValidos)}");
            }

            // Usar los permisos predefinidos según el rol
            Permiso permisoDomain;
            if (dto.Rol.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                permisoDomain = new Permiso();
                permisoDomain.SetConfigurarcionAdmin();
            }
            else // Usuario por defecto
            {
                permisoDomain = new Permiso();
                permisoDomain.SetConfigurarcionUsuario();
            }

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

        public async Task<IEnumerable<PermisoDto>> ObtenerPorRolAsync(string rol, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(rol)) return Array.Empty<PermisoDto>();
            
            // Validar que el rol sea válido
            if (!Permiso.EsRolValido(rol))
            {
                throw new ArgumentException($"Rol '{rol}' no válido. Los roles permitidos son: {string.Join(", ", Permiso.RolesValidos)}");
            }

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

            // Validar rol si se está actualizando
            if (dto.Rol != null && !Permiso.EsRolValido(dto.Rol))
            {
                throw new ArgumentException($"Rol '{dto.Rol}' no válido. Los roles permitidos son: {string.Join(", ", Permiso.RolesValidos)}");
            }

            // Si se está cambiando el rol, aplicar la configuración predefinida
            if (dto.Rol != null && dto.Rol != existing.Rol)
            {
                var permisoDomain = new Permiso();
                if (dto.Rol.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    permisoDomain.SetConfigurarcionAdmin();
                }
                else
                {
                    permisoDomain.SetConfigurarcionUsuario();
                }
                
                existing.Rol = permisoDomain.Rol;
                existing.CrearTarea = permisoDomain.CrearTarea;
                existing.EliminarTarea = permisoDomain.EliminarTarea;
                existing.EditarTarea = permisoDomain.EditarTarea;
                existing.AñadirUsuario = permisoDomain.AñadirUsuario;
                existing.EliminarUsuario = permisoDomain.EliminarUsuario;
                existing.AsignarTarea = permisoDomain.AsignarTarea;
                existing.AsignarseTarea = permisoDomain.AsignarseTarea;
            }
            else
            {
                // Actualizar solo campos no nulos si no se está cambiando el rol
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
                await _repo.DeleteAsync(id, ct);
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
