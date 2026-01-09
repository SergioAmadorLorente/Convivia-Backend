using Convivia.Application.Repositories;
using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Convivia.Application.Services
{
    public class UsuarioEspacioService
    {
        private readonly IUsuarioEspacioRepository _usuarioEspacioRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UsuarioEspacioService> _logger;
        private readonly IFacturaRepository _facturaRepo;
        private readonly ITareaRepository _tareaRepo;

        public UsuarioEspacioService(IUsuarioEspacioRepository repo,IMapper mapper, ILogger<UsuarioEspacioService> logger, IFacturaRepository facturaRepo, ITareaRepository tareaRepo)
        {
            _usuarioEspacioRepository = repo ?? throw new ArgumentNullException(nameof(repo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _facturaRepo = facturaRepo ?? throw new ArgumentNullException(nameof(facturaRepo)); 
            _tareaRepo = tareaRepo ?? throw new ArgumentNullException(nameof(tareaRepo));
        }

        // Crear UsuarioEspacio
        
        public async Task<UsuarioEspacioDto?> CrearUsuarioEspacioAsync(CreateUsuarioEspacioDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (dto.Ausente == null) throw new ArgumentException("Ausente no puede ser nulo", nameof(dto.Ausente));

            if (dto.Karma < 0) throw new ArgumentException("Karma no puede ser negativo", nameof(dto.Karma));
            if (string.IsNullOrWhiteSpace(dto.Rol)) throw new ArgumentException("Rol no puede estar vacío", nameof(dto.Rol));
            if (string.IsNullOrWhiteSpace(dto.EspacioId)) throw new ArgumentException("EspacioId no puede estar vacío", nameof(dto.EspacioId));
            if (string.IsNullOrWhiteSpace(dto.UsuarioId)) throw new ArgumentException("UsuarioId no puede estar vacío", nameof(dto.UsuarioId));
            if (dto.TareasId == null) throw new ArgumentException("TareasId no puede ser nulo", nameof(dto.TareasId));
            //if (string.IsNullOrWhiteSpace(dto.PermisoId)) throw new ArgumentException("PermisoId no puede estar vacío", nameof(dto.PermisoId));


            // DTO -> Domain
            var UsuarioEspacioDomain = _mapper.Map<UsuarioEspacio>(dto);

            // Persistir y obtener id           
            var id = await _usuarioEspacioRepository.AddAsync(UsuarioEspacioDomain, ct);

            // Recuperar entidad guardada y devolver DTO consistente
            var createdDomain = await _usuarioEspacioRepository.GetByIdAsync(id, ct);
            if (createdDomain == null)
            {
                // devolver DTO mínimo con id para evitar fallos en rutas
                return new UsuarioEspacioDto { Id = id };
            }

            var createdDto = _mapper.Map<UsuarioEspacioDto>(createdDomain);
            if (string.IsNullOrWhiteSpace(createdDto.Id))
                createdDto.Id = id;

            return createdDto;
        }

        // Obtener UsuarioEspacio por Id
        public async Task<UsuarioEspacioDto?> ObtenerUsuarioEspacioAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            var domain = await _usuarioEspacioRepository.GetByIdAsync(id, ct);
            return domain == null ? null : _mapper.Map<UsuarioEspacioDto>(domain);
        }

        // Obtener UsuariosEspacios por EspacioId (lista)
        public async Task<IEnumerable<UsuarioEspacioDto>> ObtenerPorEspacioAsync(string espacioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return Enumerable.Empty<UsuarioEspacioDto>();
            var list = await _usuarioEspacioRepository.GetByEspacioIdAsync(espacioId, ct);
            return list?.Select(f => _mapper.Map<UsuarioEspacioDto>(f)).ToList() ?? new List<UsuarioEspacioDto>();
        }

        // Obtener UsuarioEspacio por UsuarioId (único)
        public async Task<IEnumerable<UsuarioEspacioDto>> ObtenerPorUsuarioAsync(string usuarioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioId)) return Enumerable.Empty<UsuarioEspacioDto>();
            var list = await _usuarioEspacioRepository.GetByUsuarioIdAsync(usuarioId, ct);
            return list?.Select(f => _mapper.Map<UsuarioEspacioDto>(f)).ToList() ?? new List<UsuarioEspacioDto>();
        }

        // Obtener todos
        public async Task<IEnumerable<UsuarioEspacioDto>> ListarTodasAsync(CancellationToken ct = default)
        {
            var list = await _usuarioEspacioRepository.GetAllAsync(ct);
            return list?.Select(f => _mapper.Map<UsuarioEspacioDto>(f)).ToList() ?? new List<UsuarioEspacioDto>();
        }

        // Actualizar UsuarioEspacio
        public async Task<UsuarioEspacioDto> ActualizarUsuarioEspacioCompletoAsync(string id, UpdateUsuarioEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Mapear DTO -> Domain (nuevo objeto completo)
            var domain = _mapper.Map<UsuarioEspacio>(dto);

            // Asegurar que el Id de dominio coincide con el id pasado
            domain.Id = id;

            // Persistir como overwrite (merge = false)
            await _usuarioEspacioRepository.UpdateAsync(id, domain, merge: false, ct);

            var updated = await _usuarioEspacioRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<UsuarioEspacioDto>(updated);
        }

        /// <summary>
        /// Merge: fusiona los campos del objeto con los del documento existente (SetOptions.MergeAll).
        /// </summary>
        public async Task<UsuarioEspacioDto?> ActualizarUsuarioEspacioMergeAsync(string id, UpdateUsuarioEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _usuarioEspacioRepository.GetByIdAsync(id, ct);
            if (existing == null) return null;

            // Mapear DTO sobre la entidad existente (Mapster configurado para IgnoreNullValues)
            _mapper.Map(dto, existing);

            // Persistir con merge para evitar sobrescribir campos no mapeados
            await _usuarioEspacioRepository.UpdateAsync(id, existing, merge: true, ct);

            var updated = await _usuarioEspacioRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<UsuarioEspacioDto>(updated);
        }

        // Actualización parcial
        public async Task<UsuarioEspacioDto?> ActualizarUsuarioEspacioParcialAsync(string id, UpdateUsuarioEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var updates = ObtenerActualizacionesDesdeDto(dto);
            if (updates.Count == 0)
            {
                // Nada que actualizar: devolver la entidad actual
                var current = await _usuarioEspacioRepository.GetByIdAsync(id, ct);
                return current == null ? null : _mapper.Map<UsuarioEspacioDto>(current);
            }

            // useSetMerge: false -> UpdateAsync estricto (fallará si no existe)
            await _usuarioEspacioRepository.UpdateAsync(id, updates, useSetMerge: false, ct);

            var updated = await _usuarioEspacioRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<UsuarioEspacioDto>(updated);
        }

        // Eliminar UsuarioEspacio
        public async Task<bool> EliminarUsuarioEspacioAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            if (await _facturaRepo.ExistsByUsuarioEspacioIdAsync(id, ct))
                throw new InvalidOperationException($"No se puede eliminar el UsuarioEspacio {id}: existen facturas asociadas.");

            var tareas = await _tareaRepo.GetByUsuarioEspacioIdAsync(id, ct);

            foreach (var tarea in tareas)
            {
                tarea!.UsuarioEspacioId = null;
                await _tareaRepo.UpdateAsync(tarea.Id, tarea, ct);
            }
            

            var existing = await _usuarioEspacioRepository.GetByIdAsync(id, ct);
            if (existing == null) return false;

            await _usuarioEspacioRepository.DeleteAsync(id, ct);
            return true;
        }
        private IDictionary<string, object> ObtenerActualizacionesDesdeDto(UpdateUsuarioEspacioDto dto)
        {
            var updates = new Dictionary<string, object>();

            if (dto.Ausente != null) updates["Ausente"] = dto.Ausente;
            if (dto.Karma.HasValue) updates["Karma"] = dto.Karma.Value;
            if (!string.IsNullOrWhiteSpace(dto.Rol)) updates["Rol"] = dto.Rol;
            if (!string.IsNullOrWhiteSpace(dto.EspacioId)) updates["EspacioId"] = dto.EspacioId;
            if (!string.IsNullOrWhiteSpace(dto.UsuarioId)) updates["UsuarioId"] = dto.UsuarioId;
            if (dto.TareasId != null) updates["TareasId"] = dto.TareasId;
            if (!string.IsNullOrWhiteSpace(dto.PermisoId)) updates["PermisoId"] = dto.PermisoId;
            if (dto.FacturasId != null) updates["FacturasId"] = dto.FacturasId;
            return updates;
        }
    }
}
