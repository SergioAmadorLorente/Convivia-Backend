using Convivia.Application.Repositories;
using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using Convivia.Shared.Services;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Application.Services
{
    public class EspacioService
    {
        private readonly IEspacioRepository _espacioRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EspacioService> _logger;

        public EspacioService(IEspacioRepository espacioRepository, IMapper mapper, ILogger<EspacioService> logger)
        {
            _espacioRepository = espacioRepository ?? throw new ArgumentNullException(nameof(espacioRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<EspacioDto> CrearEspacioAsync(CreateEspacioDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("Nombre requerido");

            // DTO -> domain
            var espacioDomain = _mapper.Map<Espacio>(dto);

            // Persistir y obtener id
            var id = await _espacioRepository.AddAsync(espacioDomain, ct);

            // Recuperar entidad guardada y devolver DTO consistente
            var createdDomain = await _espacioRepository.GetByIdAsync(id, ct);
            if (createdDomain == null)
            {
                // devolver DTO mínimo con id para evitar fallos en rutas
                return new EspacioDto { IdEspacio = id };
            }

            var createdDto = _mapper.Map<EspacioDto>(createdDomain);
            if (string.IsNullOrWhiteSpace(createdDto.IdEspacio))
                createdDto.IdEspacio = id;

            return createdDto;
            
        }

        /// <summary>
        /// Obtiene una espacio por id.
        /// </summary>
        public async Task<EspacioDto?> ObtenerEspacioAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            var domain = await _espacioRepository.GetByIdAsync(id, ct);
            return domain == null ? null : _mapper.Map<EspacioDto>(domain);
        }


        /// <summary>
        /// Lista todas las espacios.
        /// </summary>
        public async Task<List<EspacioDto>> ListarTodasAsync(CancellationToken ct = default)
        {
            var list = await _espacioRepository.GetAllAsync(ct);
            return list?.Select(f => _mapper.Map<EspacioDto>(f)).ToList() ?? new List<EspacioDto>();
        }

        public async Task<IEnumerable<Espacio>> ObtenerPorDireccionAsync(string direccion, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(direccion)) return Enumerable.Empty<Espacio>();
            try
            {
                return await _espacioRepository.GetByDireccionAsync(direccion, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorDireccion {Direccion}", direccion);
                throw;
            }
        }

        // Overwrite completo: reemplaza todo el documento en Firestore (PUT)
        public async Task<EspacioDto?> ActualizarEspacioCompletoAsync(string id, UpdateEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Mapear DTO -> Domain (suponiendo que tienes la entidad Espacio)
            var domain = _mapper.Map<Espacio>(dto);
            domain.Id = id; // asegurar id

            // Persistir como overwrite 
            await _espacioRepository.UpdateAsync(id, domain, merge: false, ct);

            var updated = await _espacioRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<EspacioDto>(updated);
        }

        // Merge: mapear sobre la entidad existente y persistir con merge (Set + MergeAll)
        public async Task<EspacioDto?> ActualizarEspacioMergeAsync(string id, UpdateEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Obtener existente para mapear sobre él y evitar perder campos no enviados
            var existing = await _espacioRepository.GetByIdAsync(id, ct);
            if (existing == null) return null;

            // Mapear solo valores no nulos (asegúrate de configurar Mapster: IgnoreNullValues = true)
            _mapper.Map(dto, existing);

            await _espacioRepository.UpdateAsync(id, existing, merge: true, ct);

            var updated = await _espacioRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<EspacioDto>(updated);
        }

        // Parcial / PATCH: actualiza solo los campos permitidos (construye IDictionary<string, object>)
        public async Task<EspacioDto?> ActualizarEspacioParcialAsync(string id, UpdateEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Construir diccionario de actualizaciones (usa el helper que definiste)
            var updates = ObtenerActualizacionesDesdeDto(dto); // método estático/privado que devuelve IDictionary<string, object>
            if (updates.Count == 0)
            {
                // Nada que actualizar: devolver la entidad actual
                var current = await _espacioRepository.GetByIdAsync(id, ct);
                return current == null ? null : _mapper.Map<EspacioDto>(current);
            }

          
            await _espacioRepository.UpdateAsync(id, updates, useSetMerge: false, ct);

            var updated = await _espacioRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<EspacioDto>(updated);             
        }


        /// <summary>
        /// Elimina una espacio.
        /// </summary>
        public async Task<bool> EliminarEsapcioAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido");

            var existing = await _espacioRepository.GetByIdAsync(id, ct);
            if (existing == null) return false;



            await _espacioRepository.DeleteAsync(id, ct);
            return true;
        }




        /// <summary>
        /// Construye el diccionario de actualizaciones para PATCH de Espacio.
        /// - Añade solo propiedades no nulas/no vacías del DTO.
        /// - Usa los nombres exactos de Firestore según FireStoreEspacio.
        /// - Filtra campos que no deben actualizarse por PATCH: "Id".
        /// - Normaliza cadenas (Trim) y aplica validaciones mínimas (longitud).
        /// </summary>
        public static IDictionary<string, object> ObtenerActualizacionesDesdeDto(UpdateEspacioDto dto)
        {
            var updates = new Dictionary<string, object>();
            if (dto == null) return updates;

            // Nombre: actualizar solo si se envía un valor no vacío
            if (!string.IsNullOrWhiteSpace(dto.Nombre))
            {
                var nombre = dto.Nombre.Trim();
                const int maxNombre = 200;
                if (nombre.Length == 0)
                    throw new ArgumentException("Nombre no puede quedar vacío.");
                if (nombre.Length > maxNombre)
                    throw new ArgumentException($"Nombre demasiado largo. Máximo {maxNombre} caracteres.");
                updates["Nombre"] = nombre;
            }

            // Direccion: permitir cadena vacía intencionada; excluir null
            if (dto.Direccion != null)
            {
                var direccion = dto.Direccion.Trim();
                const int maxDireccion = 1000;
                if (direccion.Length > maxDireccion)
                    throw new ArgumentException($"Dirección demasiado larga. Máximo {maxDireccion} caracteres.");
                updates["Direccion"] = direccion;
            }

            return updates;
        }
    }
}