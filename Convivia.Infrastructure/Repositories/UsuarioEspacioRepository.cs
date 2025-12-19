using Convivia.Domain.Entities;
using Convivia.Domain.Repositories;
using Convivia.Shared.Services;
using Google.Cloud.Firestore;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Infrastructure.Repositories
{
    public class UsuarioEspacioRepository : IUsuarioEspacioRepository
    {
        private const string COLLECTION = "usuariosespacios";
        private readonly IFirebaseService _firebase;
        private readonly ILogger<UsuarioEspacioRepository> _logger;

        public UsuarioEspacioRepository(IFirebaseService firebase, ILogger<UsuarioEspacioRepository> logger)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UsuarioEspacio?> GetByIdAsync(string usuarioEspacioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioEspacioId))
                throw new ArgumentNullException(nameof(usuarioEspacioId));

            try
            {
                var firestoreEntity = await _firebase.GetAsync<FireStoreUsuarioEspacio>(COLLECTION, usuarioEspacioId, ct);
                if (firestoreEntity == null)
                    return null;

                return firestoreEntity.Adapt<UsuarioEspacio>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtiendo UsuarioEspacio {Id}", usuarioEspacioId);
                return null;
            }
        }

        public async Task<UsuarioEspacio?> GetByUsuarioAndEspacioAsync(string usuarioId, string espacioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new ArgumentNullException(nameof(usuarioId));
            if (string.IsNullOrWhiteSpace(espacioId))
                throw new ArgumentNullException(nameof(espacioId));

            try
            {
                var conditions = new (string field, object val)[]
                {
                    ("UsuarioId", usuarioId),
                    ("EspacioId", espacioId)
                };

                var firestoreEntities = await _firebase.QueryMultipleConditionsAsync<FireStoreUsuarioEspacio>(
                    COLLECTION, conditions, ct);

                if (firestoreEntities == null || !firestoreEntities.Any())
                    return null;

                return firestoreEntities.First().Adapt<UsuarioEspacio>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetByUsuarioAndEspacioAsync usuario={UsuarioId} espacio={EspacioId}",
                    usuarioId, espacioId);
                return null;
            }
        }

        public async Task UpdateAsync(string usuarioEspacioId, UsuarioEspacio usuarioEspacio, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioEspacioId))
                throw new ArgumentNullException(nameof(usuarioEspacioId));
            if (usuarioEspacio == null)
                throw new ArgumentNullException(nameof(usuarioEspacio));

            try
            {
                var firestoreEntity = usuarioEspacio.Adapt<FireStoreUsuarioEspacio>();
                await _firebase.UpdateAsync(COLLECTION, usuarioEspacioId, firestoreEntity, cancellationToken: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando UsuarioEspacio {Id}", usuarioEspacioId);
                throw;
            }
        }

        /// <summary>
        /// Incrementa el karma de un UsuarioEspacio usando Firestore FieldValue.Increment
        /// </summary>
        public async Task<int> UpdateKarmaAsync(string usuarioEspacioId, int karmaAmount, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioEspacioId))
                throw new ArgumentNullException(nameof(usuarioEspacioId));

            if (karmaAmount <= 0)
                throw new ArgumentException("karmaAmount debe ser mayor a 0", nameof(karmaAmount));

            try
            {
                var updates = new Dictionary<string, object>
                {
                    { "karma", FieldValue.Increment(karmaAmount) }
                };

                await _firebase.UpdateAsync(COLLECTION, usuarioEspacioId, updates, useSetMerge: true, ct);
                
                _logger.LogDebug("Karma {Amount} agregado al UsuarioEspacio {Id}", karmaAmount, usuarioEspacioId);
                
                // Retornar el karma agregado
                return karmaAmount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementando Karma para UsuarioEspacio {Id}", usuarioEspacioId);
                throw;
            }
        }
    }
}
