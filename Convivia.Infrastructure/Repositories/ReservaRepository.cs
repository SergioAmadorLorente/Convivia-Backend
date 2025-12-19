using Convivia.Domain.Entities;
using Convivia.Application.Repositories;
using Convivia.Infrastructure.Models;
using Convivia.Shared.Services;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Convivia.Infrastructure.Repositories
{
    public class ReservaRepository : Repository<FireStoreReserva>, IReservaRepository
    {
        private readonly ILogger<ReservaRepository> _logger;
        private const string Collection = "reservas";

        public ReservaRepository(IFirebaseService firebase, ILogger<ReservaRepository> logger, ILoggerFactory loggerFactory)
            : base(firebase, loggerFactory.CreateLogger<Repository<FireStoreReserva>>(), Collection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(Reserva entity, CancellationToken ct = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var persist = entity.Adapt<FireStoreReserva>();
            return await base.AddAsync(persist, ct);
        }

        public async Task<Reserva?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var persist = await base.GetByIdAsync(id, ct);
            if (persist == null) return null;
            return persist.Adapt<Reserva>();
        }

        public async Task<IEnumerable<Reserva>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await base.GetAllAsync(ct);
            return list == null ? Array.Empty<Reserva>() : list.Adapt<List<Reserva>>();
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

        public async Task UpdateAsync(string id, Reserva entity, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var persist = entity.Adapt<FireStoreReserva>();
            await base.UpdateAsync(id, persist, ct);
        }

        public async Task UpdateAsync(string id, Reserva entity, bool merge, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var persist = entity.Adapt<FireStoreReserva>();
            await base.UpdateAsync(id, persist, merge, ct);
        }

        public async Task UpdateAsync(string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default)
        {
            await base.UpdateAsync(id, updates, useSetMerge, ct);
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            await base.DeleteAsync(id, ct);
        }
    }
}