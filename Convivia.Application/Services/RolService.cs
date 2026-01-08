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
            Rol rolDomain = new Rol();
            
            switch (dto.Nombre)
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

            // Persistir y obtener id
            var id = await _rolRepository.AddAsync(rolDomain, ct);

            // Recuperar entidad guardada y devolver DTO consistente
            var createdDomain = await _rolRepository.GetByIdAsync(id, ct);
            if (createdDomain == null)
            {
                return new RolDto { Id = id };
            }

            var createdDto = _mapper.Map<RolDto>(createdDomain);
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
            return domain == null ? null : _mapper.Map<RolDto>(domain);
        }

        /// <summary>
        /// Lista todos los roles.
        /// </summary>
        public async Task<List<RolDto>> ListarTodasAsync(CancellationToken ct = default)
        {
            var list = await _rolRepository.GetAllAsync(ct);
            return list?.Select(f => _mapper.Map<RolDto>(f)).ToList() ?? new List<RolDto>();
        }

        /// <summary>
        /// Obtiene un rol por nombre (tipo).
        /// </summary>
        public async Task<RolDto?> ObtenerPorNombreAsync(TipoRol nombre, CancellationToken ct = default)
        {
            try
            {
                var domain = await _rolRepository.GetByNombreAsync(nombre, ct);
                return domain == null ? null : _mapper.Map<RolDto>(domain);
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

            // Mapear DTO -> Domain (nuevo objeto completo)
            var domain = _mapper.Map<Rol>(dto);

            // Asegurar que el Id de dominio coincide con el id pasado
            domain.Id = id;

            // Persistir como overwrite (merge = false)
            await _rolRepository.UpdateAsync(id, domain, merge: false, ct);

            var updated = await _rolRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<RolDto>(updated);
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

            // Mapear DTO sobre la entidad existente
            _mapper.Map(dto, existing);

            // Persistir con merge
            await _rolRepository.UpdateAsync(id, existing, merge: true, ct);

            var updated = await _rolRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<RolDto>(updated);
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
    }
}
