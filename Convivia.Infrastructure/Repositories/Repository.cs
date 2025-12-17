using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.Services;
using Microsoft.Extensions.Logging;

namespace Convivia.Infrastructure.Repositories
{
    // Note: This Repository<T> is an infrastructure helper operating on Firestore models.
    // It intentionally does NOT implement the application-level IRepository<T> to avoid
    // type mismatches between Firestore models and domain entities.
    public class Repository<T> where T : class
    {
        protected readonly IFirebaseService _firebase;
        protected readonly ILogger<Repository<T>> _logger;
        protected readonly string _collection;

        public Repository(IFirebaseService firebase, ILogger<Repository<T>> logger, string collection)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _collection = string.IsNullOrWhiteSpace(collection) ? throw new ArgumentNullException(nameof(collection)) : collection;
        }

        public virtual async Task<T?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                return await _firebase.GetAsync<T>(_collection, id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByIdAsync {Id} en colección {Collection}", id, _collection);
                throw;
            }
        }

        public virtual async Task<string> AddAsync(T entitie, CancellationToken ct = default)
        {
            if (entitie == null) throw new ArgumentNullException(nameof(entitie));
            try
            {
                return await _firebase.AddAsync(_collection, entitie, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error AddAsync en colección {Collection}", _collection);
                throw;
            }
        }

        /// <summary>
        /// Update completo (overwrite por defecto).
        /// </summary>
        public virtual async Task UpdateAsync(string id, T entitie, CancellationToken ct = default)
        {
            await UpdateAsync(id, entitie, merge: false, ct);
        }

        /// <summary>
        /// Update con opción merge (SetOptions.MergeAll).
        /// </summary>
        public virtual async Task UpdateAsync(string id, T entitie, bool merge, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (entitie == null) throw new ArgumentNullException(nameof(entitie));

            try
            {
                await _firebase.UpdateAsync(_collection, id, entitie, merge, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error UpdateAsync {Id} (merge={Merge}) en colección {Collection}", id, merge, _collection);
                throw;
            }
        }

        /// <summary>
        /// Update parcial usando diccionario de campos (útil para PATCH).
        /// </summary>
        public virtual async Task UpdateAsync(string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (updates == null) throw new ArgumentNullException(nameof(updates));
            if (updates.Count == 0) return;

            try
            {
                await _firebase.UpdateAsync(_collection, id, updates, useSetMerge, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error UpdateAsync (parcial) {Id} en colección {Collection}", id, _collection);
                throw;
            }
        }

        public virtual async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            try
            {
                await _firebase.DeleteAsync(_collection, id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error DeleteAsync {Id} en colección {Collection}", id, _collection);
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                return await _firebase.GetAllAsync<T>(_collection, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetAllAsync en colección {Collection}", _collection);
                throw;
            }
        }
    }
}