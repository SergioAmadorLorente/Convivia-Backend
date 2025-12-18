using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Microsoft.Extensions.Logging;
using Convivia.Shared.Repositories;
using Convivia.Domain.Repositories;
using Mapster;
using Convivia.Domain.Entities;

namespace Convivia.Application.Services
{
    public class PermisoService
    {
        private readonly IPermisoRepository _repo;
        private readonly IUsuarioEspacioRepository _usuarioEspacioRepo;
        private readonly ILogger<PermisoService> _logger;

        public PermisoService(IPermisoRepository repo, IUsuarioEspacioRepository usuarioEspacioRepo, ILogger<PermisoService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _usuarioEspacioRepo = usuarioEspacioRepo ?? throw new ArgumentNullException(nameof(usuarioEspacioRepo));
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
                
                switch (dto.Rol.Value)
                {
                    case TipoRol.Admin:
                        rolDomain.SetConfiguracionAdmin();
                        break;
                    case TipoRol.Moderador:
                        rolDomain.SetConfiguracionModerador();
                        break;
                    case TipoRol.Usuario:
                    default:
                        rolDomain.SetConfiguracionUsuario();
                        break;
                }
                
                existing.Rol = dto.Rol.Value;
                existing.CrearTarea = rolDomain.CrearTarea;
                existing.EliminarTarea = rolDomain.EliminarTarea;
                existing.EditarTarea = rolDomain.EditarTarea;
                existing.AñadirUsuario = rolDomain.AñadirUsuario;
                existing.EliminarUsuario = rolDomain.EliminarUsuario;
                existing.AsignarTarea = rolDomain.AsignarTarea;
                existing.AsignarseTarea = rolDomain.AsignarseTarea;
                existing.EliminarResidencia = rolDomain.EliminarResidencia;
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
                if (dto.EliminarResidencia.HasValue) existing.EliminarResidencia = dto.EliminarResidencia.Value;
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

        /// <summary>
        /// Elimina un permiso verificando integridad referencial.
        /// Si hay UsuarioEspacios asociados, lanza InvalidOperationException.
        /// </summary>
        public async Task<bool> EliminarAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            
            try
            {
                var permiso = await ObtenerPorIdAsync(id, ct);
                if (permiso == null)
                {
                    _logger.LogWarning("Intento de eliminar permiso inexistente {Id}", id);
                    return false;
                }

                // Verificar si hay usuarios asociados a este permiso
                var usuariosAsociados = await _usuarioEspacioRepo.GetByPermisoIdAsync(id, ct);
                var usuariosList = usuariosAsociados?.ToList() ?? new List<UsuarioEspacio>();
                
                if (usuariosList.Any())
                {
                    var cantidad = usuariosList.Count;
                    _logger.LogWarning("No se puede eliminar el permiso {Id} porque tiene {Cantidad} usuarios asociados", id, cantidad);
                    throw new InvalidOperationException($"No se puede eliminar el permiso porque esta asignado a {cantidad} usuarios.");
                }

                await _repo.DeleteAsync(id, ct);
                _logger.LogInformation("Permiso {Id} eliminado exitosamente", id);
                return true;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error EliminarPermiso {Id}", id);
                throw;
            }
        }
    }
}
