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
        private static readonly string[] RolesValidos = { "Usuario", "Admin" };

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
            if (!EsRolValido(dto.Rol))
            {
                throw new ArgumentException($"Rol '{dto.Rol}' no válido. Los roles permitidos son: {string.Join(", ", RolesValidos)}");
            }

            // Mapster convierte CreatePermisoDto -> Permiso
            // RolTypeConverter maneja string -> Rol automáticamente
            var permisoDomain = dto.Adapt<Permiso>();

            // DEBUG: Verificar que el Rol esté correctamente configurado
            _logger.LogInformation("Permiso creado - Rol.Nombre: {Nombre}, CrearTarea: {CrearTarea}, EliminarTarea: {EliminarTarea}", 
                permisoDomain.Rol?.Nombre, 
                permisoDomain.Rol?.CrearTarea, 
                permisoDomain.Rol?.EliminarTarea);

            // Mapster convierte Permiso -> PermisoDto
            var permisoDto = permisoDomain.Adapt<PermisoDto>();

            // DEBUG: Verificar que el DTO tenga los valores correctos
            _logger.LogInformation("PermisoDto - Rol: {Rol}, CrearTarea: {CrearTarea}, EliminarTarea: {EliminarTarea}", 
                permisoDto.Rol, 
                permisoDto.CrearTarea, 
                permisoDto.EliminarTarea);

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
            if (!EsRolValido(rol))
            {
                throw new ArgumentException($"Rol '{rol}' no válido. Los roles permitidos son: {string.Join(", ", RolesValidos)}");
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

            if (dto.Rol != null && !EsRolValido(dto.Rol))
            {
                throw new ArgumentException($"Rol '{dto.Rol}' no válido. Los roles permitidos son: {string.Join(", ", RolesValidos)}");
            }

            // Si se cambia el rol, actualizar todo según la configuración del rol
            if (dto.Rol != null && dto.Rol != existing.Rol)
            {
                var rolDomain = new Rol();
                if (dto.Rol.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    rolDomain.SetConfigurarcionAdmin();
                }
                else
                {
                    rolDomain.SetConfigurarcionUsuario();
                }
                
                existing.Rol = rolDomain.Nombre;
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
                await _repo.DeleteAsync(id, ct);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error EliminarPermiso {Id}", id);
                throw;
            }
        }

        private static bool EsRolValido(string rol)
        {
            return !string.IsNullOrWhiteSpace(rol) && 
                   RolesValidos.Contains(rol, StringComparer.OrdinalIgnoreCase);
        }
    }
}
