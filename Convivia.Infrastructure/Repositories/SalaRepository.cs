using Convivia.Domain.Models;
using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Microsoft.Extensions.Logging;

namespace Convivia.Infrastructure.Repositories
{
    public class SalaRepository : ISalaRepository
    {
        private readonly IFirebaseService _firebase;
        private readonly ILogger<SalaRepository> _logger;
        private const string Collection = "salas";

        public SalaRepository(IFirebaseService firebase, ILogger<SalaRepository> logger)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(SalaDto sala, CancellationToken ct = default)
        {
            if (sala == null) throw new ArgumentNullException(nameof(sala));
            if (string.IsNullOrWhiteSpace(sala.Id))
            {
                var generatedId = await _firebase.AddAsync(Collection, sala, ct);
                return generatedId;
            }
            await _firebase.AddAsync(Collection, sala.Id, sala, ct);
            return sala.Id;
        }

        public async Task<SalaDto?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                return await _firebase.GetAsync<SalaDto>(Collection, id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByIdAsync {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<SalaDto>> GetByEspacioIdAsync(string idEspacio, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(idEspacio)) return Array.Empty<SalaDto>();
            try
            {
                var list = await _firebase.QueryAsync<SalaDto>(Collection, nameof(SalaDto.IdEspacio), ct);
                return list ?? new List<SalaDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByEspacioIdAsync {EspacioId}", idEspacio);
                throw;
            }
        }

        public async Task UpdateAsync(string id, SalaDto sala, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));
            if (sala == null) throw new ArgumentNullException(nameof(sala));

            try
            {
                await _firebase.UpdateAsync(Collection, id, sala, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error UpdateAsync {Id}", id);
                throw;
            }
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
}
    }
