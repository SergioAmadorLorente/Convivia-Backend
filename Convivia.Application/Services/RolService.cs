using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Microsoft.Extensions.Logging;
using Convivia.Application.Repositories;
using Mapster;
using Convivia.Domain.Entities;
using MapsterMapper;

namespace Convivia.Application.Services
{
    public class RolService
    {
        private readonly IRolRepository _rolRepository;
        private readonly IPermisoRepository _permisoRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RolService> _logger;

        public RolService(IRolRepository rolRepository, IPermisoRepository permisoRepository, IMapper mapper, ILogger<RolService> logger)
        {
            _rolRepository = rolRepository ?? throw new ArgumentNullException(nameof(rolRepository));
            _permisoRepository = permisoRepository ?? throw new ArgumentNullException(nameof(permisoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Crea un rol y devuelve el rol persistido (con Id y metadatos).
        /// </summary>
        public async Task<RolDto> CrearRolAsync(CreateRolDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Crear rol con la configuración apropiada
            Rol rolDomain;
            switch (dto.Nombre)
            {
                case TipoRol.Admin:
                    rolDomain = Rol.Admin;
                    break;
                case TipoRol.Moderador:
                    rolDomain = Rol.Moderador;
                    break;
                case TipoRol.Usuario:
                    rolDomain = Rol.Usuario;
                    break;
                default:
                    throw new ArgumentException($"Nombre de rol inválido: {dto.Nombre}");
            }

            // Persistir y obtener id
            var id = await _rolRepository.AddAsync(rolDomain, ct);

            // Recuperar entidad guardada y devolver DTO consistente
            var createdDomain = await _rolRepository.GetByIdAsync(id, ct);
            if (createdDomain == null)
            {
                return new RolDto { Id = id, Nombre = dto.Nombre };
            }

            var createdDto = _mapper.Map<RolDto>(createdDomain);
            // Asegurar que el enum Nombre se llene correctamente a partir del string guardado
            createdDto.Nombre = ParseTipoRol(createdDomain.Nombre);
            if (string.IsNullOrWhiteSpace(createdDto.Id))
                createdDto.Id = id;

            return createdDto;
        }

        /// <summary>
        /// Obtiene un rol por id.
        /// </summary>
        public async Task<RolDto?> ObtenerRolAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            var domain = await _rolRepository.GetByIdAsync(id, ct);
            if (domain == null) return null;
            var dto = _mapper.Map<RolDto>(domain);
            dto.Nombre = ParseTipoRol(domain.Nombre);
            return dto;
        }

        /// <summary>
        /// Lista todos los roles.
        /// </summary>
        public async Task<List<RolDto>> ListarTodasAsync(CancellationToken ct = default)
        {
            var list = await _rolRepository.GetAllAsync(ct);
            var dtos = list?.Select(f => {
                var d = _mapper.Map<RolDto>(f);
                d.Nombre = ParseTipoRol(f.Nombre);
                return d;
            }).ToList() ?? new List<RolDto>();
            return dtos;
        }

        /// <summary>
        /// Obtiene un rol por nombre (tipo).
        /// </summary>
        public async Task<RolDto?> ObtenerPorNombreAsync(TipoRol nombre, CancellationToken ct = default)
        {
            try
            {
                var domain = await _rolRepository.GetByNombreAsync(nombre, ct);
                if (domain == null) return null;
                var dto = _mapper.Map<RolDto>(domain);
                dto.Nombre = ParseTipoRol(domain.Nombre);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorNombre {Nombre}", nombre);
                throw;
            }
        }

        /// <summary>
        /// Overwrite completo: reemplaza todo el documento en Firestore.
        /// </summary>
        public async Task<RolDto?> ActualizarRolCompletoAsync(string id, UpdateRolDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _rolRepository.GetByIdAsync(id, ct);
            if (existing == null) return null;

            // Si se envió Permisos, aplicar sobre existente para evitar perder campos
            if (dto.Permisos != null)
            {
                ApplyPermisosToRol(existing, dto.Permisos);
            }

            // Persistir como overwrite (merge = false)
            await _rolRepository.UpdateAsync(id, existing, merge: false, ct);

            var updated = await _rolRepository.GetByIdAsync(id, ct);
            if (updated == null) return null;
            var res = _mapper.Map<RolDto>(updated);
            res.Nombre = ParseTipoRol(updated.Nombre);
            return res;
        }

        /// <summary>
        /// Merge: fusiona los campos del objeto con los del documento existente.
        /// </summary>
        public async Task<RolDto?> ActualizarRolMergeAsync(string id, UpdateRolDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _rolRepository.GetByIdAsync(id, ct);
            if (existing == null) return null;

            // Mapear DTO sobre la entidad existente (Mapster configurado para IgnoreNullValues)
            // Pero UpdateRolDto contains nested Permisos which Mapster may not map; handle explicitly
            if (dto.Permisos != null)
            {
                ApplyPermisosToRol(existing, dto.Permisos);
            }

            await _rolRepository.UpdateAsync(id, existing, merge: true, ct);

            var updated = await _rolRepository.GetByIdAsync(id, ct);
            if (updated == null) return null;
            var res = _mapper.Map<RolDto>(updated);
            res.Nombre = ParseTipoRol(updated.Nombre);
            return res;
        }

        /// <summary>
        /// Elimina un rol.
        /// </summary>
        public async Task<bool> EliminarRolAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            
            var existing = await _rolRepository.GetByIdAsync(id, ct);
            if (existing == null) return false;

            // Verificar si hay permisos asociados a este rol
            var tipoRol = ParseTipoRol(existing.Nombre);
            var permisosAsociados = await _permisoRepository.GetByRolAsync(tipoRol, ct);
            if (permisosAsociados != null && permisosAsociados.Any())
            {
                var cantidadPermisos = permisosAsociados.Count();
                _logger.LogWarning("No se puede eliminar el rol {Id} porque tiene {Cantidad} permisos asociados", id, cantidadPermisos);
                throw new InvalidOperationException($"No se puede eliminar el rol porque tiene {cantidadPermisos} usuarios asignados.");
            }

            await _rolRepository.DeleteAsync(id, ct);
            return true;
        }

        private static TipoRol ParseTipoRol(string rolName)
        {
            return rolName switch
            {
                "Admin" => TipoRol.Admin,
                "Moderador" => TipoRol.Moderador,
                _ => TipoRol.Usuario
            };
        }

        private static void ApplyPermisosToRol(Rol rol, PermisosRolDto permisos)
        {
            if (permisos == null || rol == null) return;

            if (permisos.CrearTarea.HasValue) rol.CrearTarea = permisos.CrearTarea.Value;
            if (permisos.EliminarTarea.HasValue) rol.EliminarTarea = permisos.EliminarTarea.Value;
            if (permisos.EditarTarea.HasValue) rol.EditarTarea = permisos.EditarTarea.Value;
            if (permisos.AsignarTarea.HasValue) rol.AsignarTarea = permisos.AsignarTarea.Value;
            if (permisos.AsignarseTarea.HasValue) rol.AsignarseTarea = permisos.AsignarseTarea.Value;

            // Permisos de usuarios: DTO usa AgregarUsuario, dominio usa AñadirUsuario
            if (permisos.AgregarUsuario.HasValue) rol.AñadirUsuario = permisos.AgregarUsuario.Value;
            if (permisos.EliminarUsuario.HasValue) rol.EliminarUsuario = permisos.EliminarUsuario.Value;

            if (permisos.EliminarResidencia.HasValue) rol.EliminarResidencia = permisos.EliminarResidencia.Value;
        }
    }
}
