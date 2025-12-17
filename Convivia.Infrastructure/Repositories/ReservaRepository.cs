using Convivia.Domain.Entities;
using Convivia.Domain.Repositories;
using Convivia.Infrastructure.Models;
using Convivia.Shared.Services;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Convivia.Infrastructure.Repositories
{
    public class ReservaRepository : IReservaRepository
    {
        private readonly IFirebaseService _firebase;
        private readonly ILogger<ReservaRepository> _logger;
        private const string Collection = "reservas";

        public ReservaRepository(IFirebaseService firebase, ILogger<ReservaRepository> logger)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Reserva?> AddAsync(Reserva entity, CancellationToken ct = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var fireStoreReserva = entity.Adapt<FireStoreReserva>();
            await _firebase.AddAsync(Collection, entity.idReserva, fireStoreReserva, ct);

            return entity;
        }

        public async Task<Reserva?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                var freserva = await _firebase.GetAsync<FireStoreReserva>(Collection, id, ct);
                return freserva?.Adapt<Reserva>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByIdAsync {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Reserva>> GetByUsuarioIdAsync(string usuarioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioId)) return Array.Empty<Reserva>();
            try
            {
                var list = await _firebase.QueryAsync<FireStoreReserva>(Collection, nameof(FireStoreReserva.idUser), usuarioId, ct);
                return list?.Select(fr => fr.Adapt<Reserva>()) ?? new List<Reserva>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByUsuarioIdAsync {UsuarioId}", usuarioId);
                throw;
            }
        }

        public async Task<IEnumerable<Reserva>> GetBySalaIdAsync(string salaId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(salaId)) return Array.Empty<Reserva>();
            try
            {
                var list = await _firebase.QueryAsync<FireStoreReserva>(Collection, nameof(FireStoreReserva.idSala), salaId, ct);
                return list?.Select(fr => fr.Adapt<Reserva>()) ?? new List<Reserva>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetBySalaIdAsync {SalaId}", salaId);
                throw;
            }
        }

        public async Task<Reserva?> UpdateAsync(string id, Reserva entity, CancellationToken ct = default)
        {
            var freserva = entity.Adapt<FireStoreReserva>();
            await _firebase.UpdateAsync(Collection, id, freserva, ct);

            var updated = await _firebase.GetAsync<FireStoreReserva>(Collection, id, ct);
            return updated?.Adapt<Reserva>();
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));
            try
            {
                await _firebase.DeleteAsync(Collection, id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error DeleteAsync {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Reserva>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await _firebase.GetAllAsync<FireStoreReserva>(Collection, ct);
            return list.Select(fr => fr.Adapt<Reserva>());
        }
    }
}