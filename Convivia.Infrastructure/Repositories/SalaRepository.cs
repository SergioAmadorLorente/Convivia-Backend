using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Convivia.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Mapster;

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
            
            // Convertir SalaDto → Sala (Domain) → FireStoreSala (Firestore)
            var salaDomain = sala.Adapt<Sala>();
            var salaPersist = salaDomain.Adapt<FireStoreSala>();
            
            if (string.IsNullOrWhiteSpace(salaPersist.Id))
            {
                // Si no tiene id, pedimos a Firestore que genere una id y la devolvemos
                var generatedId = await _firebase.AddAsync(Collection, salaPersist, ct);
                return generatedId;
            }

            // Si ya tiene id, lo usamos para crear el documento con ese id
            await _firebase.AddAsync(Collection, salaPersist.Id, salaPersist, ct);
            return salaPersist.Id;
        }

        public async Task<SalaDto?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                // Obtener FireStoreSala de Firestore
                var salaPersist = await _firebase.GetAsync<FireStoreSala>(Collection, id, ct);
                if (salaPersist == null) return null;
                
                // Convertir FireStoreSala → Sala (Domain) → SalaDto
                var salaDomain = salaPersist.Adapt<Sala>();
                return salaDomain.Adapt<SalaDto>();
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
                // Consultar FireStoreSala desde Firestore
                var list = await _firebase.QueryAsync<FireStoreSala>(Collection, nameof(FireStoreSala.IdEspacio), idEspacio, ct);
                if (list == null || !list.Any()) return new List<SalaDto>();
                
                // Convertir FireStoreSala → Sala (Domain) → SalaDto
                return list.Select(sp => sp.Adapt<Sala>().Adapt<SalaDto>()).ToList();
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
                // Convertir SalaDto → Sala (Domain) → FireStoreSala
                var salaDomain = sala.Adapt<Sala>();
                var salaPersist = salaDomain.Adapt<FireStoreSala>();
                
                await _firebase.UpdateAsync(Collection, id, salaPersist, ct);
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
