using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Microsoft.Extensions.Logging;
using Convivia.Application.Repositories;
using Convivia.Domain.Entities;
using MapsterMapper;

namespace Convivia.Application.Services
{
    public class PermisoService
    {
        private readonly IPermisoRepository _permisoRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PermisoService> _logger;

        public PermisoService(IPermisoRepository permisoRepository, IMapper mapper, ILogger<PermisoService> logger)
        {
            _permisoRepository = permisoRepository ?? throw new ArgumentNullException(nameof(permisoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Crea un permiso y devuelve el permiso persistido (con Id y metadatos).
        /// </summary>
        public async Task<PermisoDto> CrearPermisoAsync(CreatePermisoDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // DTO -> Domain
            var permisoDomain = _mapper.Map<Permiso>(dto);

            // Persistir y obtener id           
            var id = await _permisoRepository.AddAsync(permisoDomain, ct);

            // Recuperar entidad guardada y devolver DTO consistente
            var createdDomain = await _permisoRepository.GetByIdAsync(id, ct);
            if (createdDomain == null)
            {
                // devolver DTO m�nimo con id para evitar fallos en rutas
                return new PermisoDto { Id = id };
            }

            var createdDto = _mapper.Map<PermisoDto>(createdDomain);
            if (string.IsNullOrWhiteSpace(createdDto.Id))
                createdDto.Id = id;

            return createdDto;
        }

        /// <summary>
        /// Obtiene un permiso por id.
        /// </summary>
        public async Task<PermisoDto?> ObtenerPermisoAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            var domain = await _permisoRepository.GetByIdAsync(id, ct);
            return domain == null ? null : _mapper.Map<PermisoDto>(domain);
        }

        public async Task<IEnumerable<PermisoDto>> ObtenerPorRolAsync(TipoRol rol, CancellationToken ct = default)
        {
            try
            {
                var permisos = await _permisoRepository.GetByRolAsync(rol, ct);
                return permisos.Select(p => _mapper.Map<PermisoDto>(p)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorRol {Rol}", rol);
                throw;
            }
        }

        /// <summary>
        /// Lista todos los permisos.
        /// </summary>
        public async Task<List<PermisoDto>> ListarTodasAsync(CancellationToken ct = default)
        {
            var list = await _permisoRepository.GetAllAsync(ct);
            return list?.Select(f => _mapper.Map<PermisoDto>(f)).ToList() ?? new List<PermisoDto>();
        }

        /// <summary>
        /// Overwrite completo: reemplaza todo el documento en Firestore.
        /// Si solo env�as Rol, los dem�s permisos se resetean a los valores por defecto del rol.
        /// </summary>
        public async Task<PermisoDto?> ActualizarPermisoCompletaAsync(string id, UpdatePermisoDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Verificar que existe
            var existing = await _permisoRepository.GetByIdAsync(id, ct);
            if (existing == null) return null;

            // Mapear DTO -> Domain (nuevo objeto completo)
            var domain = _mapper.Map<Permiso>(dto);

            // Asegurar que el Id de dominio coincide con el id pasado
            domain.Id = id;

            // Persistir como overwrite (merge = false)
            await _permisoRepository.UpdateAsync(id, domain, merge: false, ct);

            var updated = await _permisoRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<PermisoDto>(updated);
        }

        /// <summary>
        /// Merge: fusiona los campos del objeto con los del documento existente.
        /// Solo modifica los campos que env�as, preservando los dem�s.
        /// </summary>
        public async Task<PermisoDto?> ActualizarPermisoMergeAsync(string id, UpdatePermisoDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _permisoRepository.GetByIdAsync(id, ct);
            if (existing == null) return null;

            // Aplicar cambios manualmente para preservar valores existentes
            if (existing.Rol == null)
                existing.Rol = new Rol();

            // Si se especifica un nuevo TipoRol, aplicar la configuraci�n base del rol
            if (dto.Rol.HasValue)
            {
                switch (dto.Rol.Value)
                {
                    case TipoRol.Admin:
                        existing.Rol.SetConfiguracionAdmin();
                        break;
                    case TipoRol.Moderador:
                        existing.Rol.SetConfiguracionModerador();
                        break;
                    default:
                        existing.Rol.SetConfiguracionUsuario();
                        break;
                }
            }

            // Sobrescribir permisos individuales si se especifican
            if (dto.CrearTarea.HasValue) existing.Rol.CrearTarea = dto.CrearTarea.Value;
            if (dto.EliminarTarea.HasValue) existing.Rol.EliminarTarea = dto.EliminarTarea.Value;
            if (dto.EditarTarea.HasValue) existing.Rol.EditarTarea = dto.EditarTarea.Value;
            if (dto.AsignarTarea.HasValue) existing.Rol.AsignarTarea = dto.AsignarTarea.Value;
            if (dto.AsignarseTarea.HasValue) existing.Rol.AsignarseTarea = dto.AsignarseTarea.Value;
            if (dto.AnadirUsuario.HasValue) existing.Rol.AñadirUsuario = dto.AnadirUsuario.Value;
            if (dto.EliminarUsuario.HasValue) existing.Rol.EliminarUsuario = dto.EliminarUsuario.Value;
            if (dto.EliminarResidencia.HasValue) existing.Rol.EliminarResidencia = dto.EliminarResidencia.Value;

            // Persistir con merge
            await _permisoRepository.UpdateAsync(id, existing, merge: true, ct);

            var updated = await _permisoRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<PermisoDto>(updated);
        }

        /// <summary>
        /// Parcial / PATCH: construye un diccionario con solo las propiedades no nulas del DTO
        /// y llama a la sobrecarga del repositorio que acepta IDictionary (update parcial).
        /// </summary>
        public async Task<PermisoDto?> ActualizarPermisoParcialAsync(string id, UpdatePermisoDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var updates = ObtenerActualizacionesDesdeDto(dto);
            if (updates.Count == 0)
            {
                // Nada que actualizar: devolver la entidad actual
                var current = await _permisoRepository.GetByIdAsync(id, ct);
                return current == null ? null : _mapper.Map<PermisoDto>(current);
            }

            // useSetMerge: false -> UpdateAsync estricto (fallar� si no existe)
            await _permisoRepository.UpdateAsync(id, updates, useSetMerge: false, ct);

            var updated = await _permisoRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<PermisoDto>(updated);
        }

        private IDictionary<string, object> ObtenerActualizacionesDesdeDto(UpdatePermisoDto dto)
        {
            var updates = new Dictionary<string, object>();
            if (dto.Rol.HasValue) updates["Rol"] = dto.Rol.Value.ToString();
            if (dto.CrearTarea.HasValue) updates["CrearTarea"] = dto.CrearTarea.Value;
            if (dto.EliminarTarea.HasValue) updates["EliminarTarea"] = dto.EliminarTarea.Value;
            if (dto.EditarTarea.HasValue) updates["EditarTarea"] = dto.EditarTarea.Value;
            if (dto.AsignarTarea.HasValue) updates["AsignarTarea"] = dto.AsignarTarea.Value;
            if (dto.AsignarseTarea.HasValue) updates["AsignarseTarea"] = dto.AsignarseTarea.Value;
            if (dto.AnadirUsuario.HasValue) updates["A�adirUsuario"] = dto.AnadirUsuario.Value;
            if (dto.EliminarUsuario.HasValue) updates["EliminarUsuario"] = dto.EliminarUsuario.Value;
            if (dto.EliminarResidencia.HasValue) updates["EliminarResidencia"] = dto.EliminarResidencia.Value;
            return updates;
        }

        /// <summary>
        /// Elimina un permiso.
        /// </summary>
        public async Task<bool> EliminarPermisoAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var existing = await _permisoRepository.GetByIdAsync(id, ct);
            if (existing == null) return false;

            await _permisoRepository.DeleteAsync(id, ct);
            return true;
        }
    }
}
