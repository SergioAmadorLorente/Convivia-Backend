using System;
using Mapster;
using Convivia.Shared.DTOs;
using Convivia.Domain.Entities;
using Convivia.Application.Repositories;
using Microsoft.Extensions.Logging;
using MapsterMapper;

namespace Convivia.Application.Services
{
    /// <summary>
    /// Servicio de aplicación para gestionar peticiones
    /// Orquesta la lógica de negocio usando el repositorio
    /// SIN acceso directo a Firebase
    /// </summary>
    public class PeticionService
    {
        private readonly IPeticionRepository _peticionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PeticionService> _logger;

        public PeticionService(IPeticionRepository peticionRepository, IMapper mapper, ILogger<PeticionService> logger)
        {
            _peticionRepository = peticionRepository ?? throw new ArgumentNullException(nameof(peticionRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Crear nueva petición
        /// </summary>
        public async Task<PeticionDto> CrearPeticionAsync(CreatePeticionDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Mensaje)) 
                throw new ArgumentException("El mensaje no puede estar vacío", nameof(dto.Mensaje));
            if (string.IsNullOrWhiteSpace(dto.IdSolicitante))
                throw new ArgumentException("El ID del solicitante no puede estar vacío", nameof(dto.IdSolicitante));
            if (string.IsNullOrWhiteSpace(dto.IdEspacio))
                throw new ArgumentException("El ID del espacio no puede estar vacío", nameof(dto.IdEspacio));

            // DTO -> Domain
            var peticionDomain = _mapper.Map<Peticion>(dto);

            // Persistir y obtener id           
            var id = await _peticionRepository.AddAsync(peticionDomain, ct);

            // Recuperar entidad guardada y devolver DTO consistente
            var createdDomain = await _peticionRepository.GetByIdAsync(id, ct);
            if (createdDomain == null)
            {
                // devolver DTO mínimo con id para evitar fallos en rutas
                return new PeticionDto { Id = id };
            }

            var createdDto = _mapper.Map<PeticionDto>(createdDomain);
            if (string.IsNullOrWhiteSpace(createdDto.Id))
                createdDto.Id = id;

            return createdDto;
        }

        /// <summary>
        /// Obtener petición por ID
        /// </summary>
        public async Task<PeticionDto?> ObtenerPeticionAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            var domain = await _peticionRepository.GetByIdAsync(id, ct);
            return domain == null ? null : _mapper.Map<PeticionDto>(domain);
        }


        /// <summary>
        /// Listar todas las peticiones
        /// </summary>
        public async Task<List<PeticionDto>> ListarTodasAsync(CancellationToken ct = default)
        {
            var list = await _peticionRepository.GetAllAsync(ct);
            return list?.Select(f => _mapper.Map<PeticionDto>(f)).ToList() ?? new List<PeticionDto>();
        }

        /// <summary>
        /// Obtener peticiones por estado
        /// </summary>
        public async Task<List<PeticionDto>> ListarPorEstadoAsync(string estado)
        {
            if (string.IsNullOrWhiteSpace(estado)) throw new ArgumentNullException(nameof(estado));
            try
            {
                var entities = await _peticionRepository.GetByEstadoAsync(estado);
                return entities.Select(p => p.Adapt<PeticionDto>()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ListarPorEstado {Estado}", estado);
                throw;
            }
        }

        /// <summary>
        /// Overwrite completo: reemplaza todo el documento en Firestore.
        /// </summary>
        public async Task<PeticionDto?> ActualizarPeticionCompletaAsync(string id, UpdatePeticionDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Mapear DTO -> Domain (nuevo objeto completo)
            var domain = _mapper.Map<Peticion>(dto);

            // Asegurar que el Id de dominio coincide con el id pasado
            domain.Id = id;

            // Persistir como overwrite (merge = false)
            await _peticionRepository.UpdateAsync(id, domain, merge: false, ct);

            var updated = await _peticionRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<PeticionDto>(updated);
        }

        /// <summary>
        /// Merge: fusiona los campos del objeto con los del documento existente (SetOptions.MergeAll).
        /// </summary>
        public async Task<PeticionDto?> ActualizarPeticionMergeAsync(string id, UpdatePeticionDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _peticionRepository.GetByIdAsync(id, ct);
            if (existing == null) return null;

            // Mapear DTO sobre la entidad existente (Mapster configurado para IgnoreNullValues)
            _mapper.Map(dto, existing);

            // Persistir con merge para evitar sobrescribir campos no mapeados
            await _peticionRepository.UpdateAsync(id, existing, merge: true, ct);

            var updated = await _peticionRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<PeticionDto>(updated);
        }

        /// <summary>
        /// Parcial / PATCH: construye un diccionario con solo las propiedades no nulas del DTO
        /// y llama a la sobrecarga del repositorio que acepta IDictionary (update parcial).
        /// </summary>
        public async Task<PeticionDto?> ActualizarPeticionParcialAsync(string id, UpdatePeticionDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var updates = ObtenerActualizacionesDesdeDto(dto);
            if (updates.Count == 0)
            {
                // Nada que actualizar: devolver la entidad actual
                var current = await _peticionRepository.GetByIdAsync(id, ct);
                return current == null ? null : _mapper.Map<PeticionDto>(current);
            }

            // useSetMerge: false -> UpdateAsync estricto (fallará si no existe)
            await _peticionRepository.UpdateAsync(id, updates, useSetMerge: false, ct);

            var updated = await _peticionRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<PeticionDto>(updated);
        }

        /// <summary>
        /// Elimina una peticion.
        /// </summary>
        public async Task<bool> EliminarPeticionAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var existing = await _peticionRepository.GetByIdAsync(id, ct);
            if (existing == null) return false;

            await _peticionRepository.DeleteAsync(id, ct);
            return true;
        }



        // Mapper manual per al patch
        private IDictionary<string, object> ObtenerActualizacionesDesdeDto(UpdatePeticionDto dto)
        {
            var updates = new Dictionary<string, object>();

            // Mapeo explícito por propiedad (seguro y claro)
            if (dto.Mensaje != null) updates["Mensaje"] = dto.Mensaje;
            if (dto.Estado != null) updates["Estado"] = dto.Estado;
            if (dto.IdSolicitante != null) updates["IdSolicitante"] = dto.IdSolicitante;
            if (dto.IdEspacio != null) updates["IdEspacio"] = dto.IdEspacio;

            return updates;
        }
    }
}
