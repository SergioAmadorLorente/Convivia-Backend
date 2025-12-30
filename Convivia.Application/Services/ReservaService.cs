using Mapster;
using Convivia.Shared.DTOs;
using Convivia.Domain.Entities;
using Convivia.Application.Repositories;
using Microsoft.Extensions.Logging;
using MapsterMapper;

namespace Convivia.Application.Services
{
    public class ReservaService
    {
        private readonly IReservaRepository _reservaRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ReservaService> _logger;

        public ReservaService(IReservaRepository reservaRepository,IMapper mapper, ILogger<ReservaService> logger)
        {
            _reservaRepository = reservaRepository ?? throw new ArgumentNullException(nameof(reservaRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Crea una Reserva y devuelve la Reserva persistida (con Id y metadatos).
        /// </summary>
        public async Task<ReservaDto> CrearReservaAsync(CreateReservaDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.idSala)) throw new ArgumentNullException(nameof(dto.idSala));
            if (string.IsNullOrWhiteSpace(dto.idUser)) throw new ArgumentNullException(nameof(dto.idUser));

            // DTO -> Domain
            var reservaDomain = _mapper.Map<Reserva>(dto);

            // Persistir y obtener id           
            var id = await _reservaRepository.AddAsync(reservaDomain, ct);

            // Recuperar entidad guardada y devolver DTO consistente
            var createdDomain = await _reservaRepository.GetByIdAsync(id, ct);
            if (createdDomain == null)
            {
                // devolver DTO mínimo con id para evitar fallos en rutas
                return new ReservaDto { Id = id };
            }

            var createdDto = _mapper.Map<ReservaDto>(createdDomain);
            if (string.IsNullOrWhiteSpace(createdDto.Id))
                createdDto.Id = id;

            return createdDto;
        }

        /// <summary>
        /// Obtiene una reserva por id.
        /// </summary>
        public async Task<ReservaDto?> ObtenerReservaAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            var domain = await _reservaRepository.GetByIdAsync(id, ct);
            return domain == null ? null : _mapper.Map<ReservaDto>(domain);
        }

        /// <summary>
        /// Lista todas las resergvas.
        /// </summary>
        public async Task<List<ReservaDto>> ListarTodasAsync(CancellationToken ct = default)
        {
            var list = await _reservaRepository.GetAllAsync(ct);
            return list?.Select(f => _mapper.Map<ReservaDto>(f)).ToList() ?? new List<ReservaDto>();
        }

        // Obtener reservas por usuario
        public async Task<IEnumerable<ReservaDto>> ObtenerPorUsuarioAsync(string idUser, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(idUser)) return Enumerable.Empty<ReservaDto>();
            var reservas = await _reservaRepository.GetByUsuarioIdAsync(idUser, ct);
            return reservas.Select(r => r.Adapt<ReservaDto>());
        }

        // Obtener reservas por sala
        public async Task<IEnumerable<ReservaDto>> ObtenerPorSalaAsync(string idSala, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(idSala)) return Enumerable.Empty<ReservaDto>();
            var reservas = await _reservaRepository.GetBySalaIdAsync(idSala, ct);
            return reservas.Select(r => r.Adapt<ReservaDto>());
        }

        /// <summary>
        /// Overwrite completo: reemplaza todo el documento en Firestore.
        /// </summary>
        public async Task<ReservaDto?> ActualizarReservaCompletaAsync(string id, UpdateReservaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Mapear DTO -> Domain (nuevo objeto completo)
            var domain = _mapper.Map<Reserva>(dto);

            // Asegurar que el Id de dominio coincide con el id pasado
            domain.Id = id;

            // Persistir como overwrite (merge = false)
            await _reservaRepository.UpdateAsync(id, domain, merge: false, ct);

            var updated = await _reservaRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<ReservaDto>(updated);
        }

        /// <summary>
        /// Merge: fusiona los campos del objeto con los del documento existente (SetOptions.MergeAll).
        /// </summary>
        public async Task<ReservaDto?> ActualizarReservaMergeAsync(string id, UpdateReservaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _reservaRepository.GetByIdAsync(id, ct);
            if (existing == null) return null;

            // Mapear DTO sobre la entidad existente (Mapster configurado para IgnoreNullValues)
            _mapper.Map(dto, existing);

            // Persistir con merge para evitar sobrescribir campos no mapeados
            await _reservaRepository.UpdateAsync(id, existing, merge: true, ct);

            var updated = await _reservaRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<ReservaDto>(updated);
        }

        /// <summary>
        /// Parcial / PATCH: construye un diccionario con solo las propiedades no nulas del DTO
        /// y llama a la sobrecarga del repositorio que acepta IDictionary (update parcial).
        /// </summary>
        public async Task<ReservaDto?> ActualizarReservaParcialAsync(string id, UpdateReservaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var updates = ObtenerActualizacionesDesdeDto(dto);
            if (updates.Count == 0)
            {
                // Nada que actualizar: devolver la entidad actual
                var current = await _reservaRepository.GetByIdAsync(id, ct);
                return current == null ? null : _mapper.Map<ReservaDto>(current);
            }

            // useSetMerge: false -> UpdateAsync estricto (fallará si no existe)
            await _reservaRepository.UpdateAsync(id, updates, useSetMerge: false, ct);

            var updated = await _reservaRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<ReservaDto>(updated);
        }

        // Eliminar reserva
        public async Task<bool> EliminarReservaAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));

            var existing = await _reservaRepository.GetByIdAsync(id, ct);
            if (existing == null) return false;
            
            await _reservaRepository.DeleteAsync(id, ct);
            return true;
        }

        // Mapper interno para el patch
        private IDictionary<string, object> ObtenerActualizacionesDesdeDto(UpdateReservaDto dto)
        {
            var updates = new Dictionary<string, object>();

            if (dto.description != null) updates["Descripcion"] = dto.description;
            if (dto.idSala != null) updates["idSala"] = dto.idSala;
            if (dto.idUser != null) updates["idUser"] = dto.idUser;

            return updates;
        }
    }
}