using Mapster;
using Convivia.Shared.DTOs;
using Convivia.Domain.Entities;
using Convivia.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace Convivia.Application.Services
{
    public class ReservaService
    {
        private readonly IReservaRepository _repo;
        private readonly ILogger<ReservaService> _logger;

        public ReservaService(IReservaRepository repo, ILogger<ReservaService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Crear reserva
        public async Task<ReservaDto?> AddAsync(CreateReservaDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var reservaDomain = dto.Adapt<Reserva>(); // La entidad genera su id automáticamente
            var createdReserva = await _repo.AddAsync(reservaDomain, ct);

            return createdReserva?.Adapt<ReservaDto>();
        }

        // Obtener reserva por id
        public async Task<ReservaDto?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            var reserva = await _repo.GetByIdAsync(id, ct);
            return reserva?.Adapt<ReservaDto>();
        }

        // Obtener reservas por usuario
        public async Task<IEnumerable<ReservaDto>> ObtenerPorUsuarioAsync(string idUser, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(idUser)) return Enumerable.Empty<ReservaDto>();
            var reservas = await _repo.GetByUsuarioIdAsync(idUser, ct);
            return reservas.Select(r => r.Adapt<ReservaDto>());
        }

        // Obtener reservas por sala
        public async Task<IEnumerable<ReservaDto>> ObtenerPorSalaAsync(string idSala, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(idSala)) return Enumerable.Empty<ReservaDto>();
            var reservas = await _repo.GetBySalaIdAsync(idSala, ct);
            return reservas.Select(r => r.Adapt<ReservaDto>());
        }

        // Actualizar reserva (update completo)
        public async Task<ReservaDto> UpdateAsync(string id, UpdateReservaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido");
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var reservaExistente = await _repo.GetByIdAsync(id, ct);
            if (reservaExistente == null) throw new KeyNotFoundException($"Reserva con id {id} no encontrada");

            reservaExistente.description = dto.description ?? reservaExistente.description;
            reservaExistente.startTime = dto.startTime ?? reservaExistente.startTime;
            reservaExistente.endTime = dto.endTime ?? reservaExistente.endTime;
            reservaExistente.idSala = dto.idSala ?? reservaExistente.idSala;
            reservaExistente.idUser = dto.idUser ?? reservaExistente.idUser;

            await _repo.UpdateAsync(id, reservaExistente, ct);
            return reservaExistente.Adapt<ReservaDto>();
        }

        // Eliminar reserva
        public async Task EliminarAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido");
            await _repo.DeleteAsync(id, ct);
        }

        // Actualización parcial
        public async Task<bool> ParcialActualizarAsync(string id, UpdateReservaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido");
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var reserva = await _repo.GetByIdAsync(id, ct);
            if (reserva == null) return false;

            var changed = false;

            if (!string.IsNullOrWhiteSpace(dto.description) && dto.description != reserva.description)
            {
                reserva.description = dto.description;
                changed = true;
            }

            if (dto.startTime.HasValue && dto.startTime != reserva.startTime)
            {
                reserva.startTime = dto.startTime.Value;
                changed = true;
            }

            if (dto.endTime.HasValue && dto.endTime != reserva.endTime)
            {
                reserva.endTime = dto.endTime.Value;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.idSala) && dto.idSala != reserva.idSala)
            {
                reserva.idSala = dto.idSala;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.idUser) && dto.idUser != reserva.idUser)
            {
                reserva.idUser = dto.idUser;
                changed = true;
            }

            if (!changed) return true; // nada que actualizar

            await _repo.UpdateAsync(id, reserva, ct);
            return true;
        }
    }
}