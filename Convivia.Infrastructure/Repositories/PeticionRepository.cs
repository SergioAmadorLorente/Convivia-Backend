using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Convivia.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Mapster;

namespace Convivia.Infrastructure.Repositories
{
    public class PeticionRepository : IPeticionRepository
    {
        private readonly IFirebaseService _firebase;
        private readonly ILogger<PeticionRepository> _logger;
        private const string Collection = "peticiones";

        public PeticionRepository(IFirebaseService firebase, ILogger<PeticionRepository> logger)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(PeticionDto peticion, CancellationToken ct = default)
        {
            if (peticion == null) throw new ArgumentNullException(nameof(peticion));

            // Convertir PeticionDto ? Peticion (Domain) ? FireStorePeticion (Firestore)
            var peticionDomain = peticion.Adapt<Peticion>();
            var peticionPersist = peticionDomain.Adapt<FireStorePeticion>();

            if (string.IsNullOrWhiteSpace(peticionPersist.Id))
            {
                var generatedId = await _firebase.AddAsync(Collection, peticionPersist, ct);
                return generatedId;
            }

            await _firebase.AddAsync(Collection, peticionPersist.Id, peticionPersist, ct);
            return peticionPersist.Id;
        }

        public async Task<PeticionDto?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                var peticionPersist = await _firebase.GetAsync<FireStorePeticion>(Collection, id, ct);
                if (peticionPersist == null) return null;

                // Convertir FireStorePeticion ? Peticion (Domain) ? PeticionDto
                var peticionDomain = peticionPersist.Adapt<Peticion>();
                return peticionDomain.Adapt<PeticionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByIdAsync {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<PeticionDto>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                var list = await _firebase.GetAllAsync<FireStorePeticion>(Collection, ct);
                if (list == null || !list.Any()) return new List<PeticionDto>();

                return list.Select(p => p.Adapt<Peticion>().Adapt<PeticionDto>()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetAllAsync");
                throw;
            }
        }

        public async Task<IEnumerable<PeticionDto>> GetByEstadoAsync(string estado, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(estado)) return Array.Empty<PeticionDto>();
            try
            {
                var list = await _firebase.QueryAsync<FireStorePeticion>(Collection, nameof(FireStorePeticion.Estado), estado, ct);
                if (list == null || !list.Any()) return new List<PeticionDto>();

                return list.Select(p => p.Adapt<Peticion>().Adapt<PeticionDto>()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByEstadoAsync {Estado}", estado);
                throw;
            }
        }

        public async Task UpdateAsync(string id, PeticionDto peticion, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));
            if (peticion == null) throw new ArgumentNullException(nameof(peticion));

            try
            {
                // Convertir PeticionDto ? Peticion (Domain) ? FireStorePeticion
                var peticionDomain = peticion.Adapt<Peticion>();
                var peticionPersist = peticionDomain.Adapt<FireStorePeticion>();

                await _firebase.UpdateAsync(Collection, id, peticionPersist, ct);
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
