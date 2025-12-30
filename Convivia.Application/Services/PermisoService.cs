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
        private readonly IPermisoRepository _permisoRepository;
        private readonly IUsuarioEspacioRepository _usuarioEspacioRepo;
        private readonly ILogger<PermisoService> _logger;

        public PermisoService(IPermisoRepository permisoRepository, IUsuarioEspacioRepository usuarioEspacioRepo, IMapper mapper, ILogger<PermisoService> logger)
        {
            _permisoRepository = permisoRepository ?? throw new ArgumentNullException(nameof(permisoRepository));
            _usuarioEspacioRepo = usuarioEspacioRepo ?? throw new ArgumentNullException(nameof(usuarioEspacioRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Crea una permiso y devuelve la permiso persistida (con Id y metadatos).
        /// </summary>
        public async Task<PermisoDto> CrearPermisoAsync(CreatePermisoDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.Rol < 0) throw new ArgumentException("Precio no puede ser negativo", nameof(dto.Rol));

            // DTO -> Domain
            var permisoDomain = _mapper.Map<Permiso>(dto);

            // Persistir y obtener id           
            var id = await _permisoRepository.AddAsync(permisoDomain, ct);

            // Recuperar entidad guardada y devolver DTO consistente
            var createdDomain = await _permisoRepository.GetByIdAsync(id, ct);
            if (createdDomain == null)
            {
                // devolver DTO mínimo con id para evitar fallos en rutas
                return new PermisoDto { Id = id };
            }

            var createdDto = _mapper.Map<PermisoDto>(createdDomain);
            if (string.IsNullOrWhiteSpace(createdDto.Id))
                createdDto.Id = id;

            return createdDto;
        }

        /// <summary>
        /// Obtiene una permiso por id.
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
                return await _permisoRepository.GetByRolAsync(rol, ct);
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
        /// </summary>
        public async Task<PermisoDto?> ActualizarPermisoCompletaAsync(string id, UpdatePermisoDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

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
        /// Merge: fusiona los campos del objeto con los del documento existente (SetOptions.MergeAll).
        /// </summary>
        public async Task<PermisoDto?> ActualizarPermisoMergeAsync(string id, UpdatePermisoDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _permisoRepository.GetByIdAsync(id, ct);
            if (existing == null) return null;

            // Mapear DTO sobre la entidad existente (Mapster configurado para IgnoreNullValues)
            _mapper.Map(dto, existing);

            // Persistir con merge para evitar sobrescribir campos no mapeados
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

            // useSetMerge: false -> UpdateAsync estricto (fallará si no existe)
            await _permisoRepository.UpdateAsync(id, updates, useSetMerge: false, ct);

            var updated = await _permisoRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<PermisoDto>(updated);
        }


        private IDictionary<string, object> ObtenerActualizacionesDesdeDto(UpdatePermisoDto dto)
        {
            var updates = new Dictionary<string, object>();
            if (dto.Rol.HasValue) updates[nameof(Permiso.Rol)] = dto.Rol.Value;
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
