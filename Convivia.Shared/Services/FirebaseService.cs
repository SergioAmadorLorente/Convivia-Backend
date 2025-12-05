using Convivia.Shared.Services;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Infrastructure.Services
{
    public class FirebaseService : IFirebaseService
    {
        private readonly FirestoreDb _db;
        private readonly ILogger<FirebaseService> _logger;

        public FirebaseService(FirestoreDb firestoreDb, ILogger<FirebaseService> logger)
        {
            _db = firestoreDb ?? throw new ArgumentNullException(nameof(firestoreDb));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddAsync<T>(string collection, string id, T entity, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("collection required", nameof(collection));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id required", nameof(id));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            await _db.Collection(collection).Document(id).SetAsync(entity, cancellationToken: cancellationToken);
        }

        // Dejar que Firestore genere id y devolverlo (y asignarlo si la entidad tiene propiedad Id)
        public async Task AddAsync<T>(string collection, T entity, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("collection required", nameof(collection));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            await _db.Collection(collection).AddAsync(entity, cancellationToken: cancellationToken);

        }

        // Método que exige la interfaz original
        public async Task UpdateAsync<T>(string collection, string id, T entity, CancellationToken cancellationToken = default)
        {
            // Delegamos a la nueva implementación que añade la opción merge (por defecto overwrite)
            await UpdateAsync(collection, id, entity, merge: false, cancellationToken);
        }

        public async Task<T?> GetAsync<T>(string collection, string id, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("collection required", nameof(collection));
            if (string.IsNullOrWhiteSpace(id)) return null;

            var snap = await _db.Collection(collection).Document(id).GetSnapshotAsync(cancellationToken);
            if (!snap.Exists) return null;

            var entity = snap.ConvertTo<T>();
            SetIdIfPossible(entity, snap.Id);
            return entity;
        }

        // Update con opción merge (por defecto overwrite para compatibilidad)
        public async Task UpdateAsync<T>(string collection, string id, T entity, bool merge = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("collection required", nameof(collection));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id required", nameof(id));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var options = merge ? SetOptions.MergeAll : SetOptions.Overwrite;
            await _db.Collection(collection).Document(id).SetAsync(entity, options, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(string collection, string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("collection required", nameof(collection));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id required", nameof(id));

            await _db.Collection(collection).Document(id).DeleteAsync(cancellationToken: cancellationToken);
        }

        public async Task<List<T>> QueryAsync<T>(string collection, string field, object value, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("collection required", nameof(collection));
            if (string.IsNullOrWhiteSpace(field)) throw new ArgumentException("field required", nameof(field));

            var snap = await _db.Collection(collection).WhereEqualTo(field, value).GetSnapshotAsync(cancellationToken);
            var result = new List<T>();
            foreach (var doc in snap.Documents)
            {
                try
                {
                    var entity = doc.ConvertTo<T>();
                    SetIdIfPossible(entity, doc.Id);
                    if (entity != null) result.Add(entity);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error converting document {DocumentId} in QueryAsync", doc.Id);
                    LogDocumentContents(doc);
                }
            }

            return result;
        }

        public async Task<List<T>> QueryMultipleConditionsAsync<T>(string collection, (string field, object val)[] conditions, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("collection required", nameof(collection));
            if (conditions == null || conditions.Length == 0) throw new ArgumentException("conditions required", nameof(conditions));

            Query q = _db.Collection(collection);
            foreach (var c in conditions)
                q = q.WhereEqualTo(c.field, c.val);

            var snap = await q.GetSnapshotAsync(cancellationToken);
            var result = new List<T>();
            foreach (var doc in snap.Documents)
            {
                try
                {
                    var entity = doc.ConvertTo<T>();
                    SetIdIfPossible(entity, doc.Id);
                    if (entity != null) result.Add(entity);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error converting document {DocumentId} in QueryMultipleConditionsAsync", doc.Id);
                    LogDocumentContents(doc);
                }
            }

            return result;
        }

        public async Task<List<T>> GetAllAsync<T>(string collection, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("collection required", nameof(collection));

            var snapshot = await _db.Collection(collection).GetSnapshotAsync(cancellationToken);
            var result = new List<T>();

            foreach (var doc in snapshot.Documents)
            {
                try
                {
                    var entity = doc.ConvertTo<T>();
                    SetIdIfPossible(entity, doc.Id);
                    if (entity != null)
                        result.Add(entity);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error converting document {DocumentId} in GetAllAsync", doc.Id);
                    LogDocumentContents(doc);
                }
            }

            return result;
        }

        // Count simple: cuidado con colecciones grandes (coste/latencia)
        public async Task<int> CountAsync(string collection, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("collection required", nameof(collection));
            var snap = await _db.Collection(collection).GetSnapshotAsync(cancellationToken);
            return snap.Documents.Count;
        }

        public async Task<List<T>> QueryCollectionGroupAsync<T>(string subcollectionName, string field, object value, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(subcollectionName)) throw new ArgumentException("subcollection required", nameof(subcollectionName));
            if (string.IsNullOrWhiteSpace(field)) throw new ArgumentException("field required", nameof(field));

            var q = _db.CollectionGroup(subcollectionName).WhereEqualTo(field, value);
            var snap = await q.GetSnapshotAsync(cancellationToken);
            var result = new List<T>();
            foreach (var doc in snap.Documents)
            {
                try
                {
                    var entity = doc.ConvertTo<T>();
                    SetIdIfPossible(entity, doc.Id);
                    if (entity != null) result.Add(entity);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error converting document {DocumentId} in QueryCollectionGroupAsync", doc.Id);
                }
            }

            return result;
        }

        public async Task<List<T>> QueryCollectionGroupAllAsync<T>(string subcollectionName, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(subcollectionName)) throw new ArgumentException("subcollection required", nameof(subcollectionName));

            var q = _db.CollectionGroup(subcollectionName);
            var snap = await q.GetSnapshotAsync(cancellationToken);
            var result = new List<T>();
            foreach (var doc in snap.Documents)
            {
                try
                {
                    var entity = doc.ConvertTo<T>();
                    SetIdIfPossible(entity, doc.Id);
                    if (entity != null) result.Add(entity);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error converting document {DocumentId} in QueryCollectionGroupAllAsync", doc.Id);
                }
            }

            return result;
        }

        // Helper: intenta asignar la propiedad Id si existe
        private void SetIdIfPossible<T>(T entity, string id)
        {
            if (entity == null || string.IsNullOrWhiteSpace(id)) return;
            try
            {
                var prop = typeof(T).GetProperty("Id");
                if (prop != null && prop.CanWrite)
                    prop.SetValue(entity, id);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "No se pudo asignar Id al tipo {TypeName}", typeof(T).FullName);
            }
        }

        private void LogDocumentContents(DocumentSnapshot doc)
        {
            try
            {
                var data = doc.ToDictionary();
                foreach (var kvp in data)
                {
                    _logger.LogError("Field: {Field}, Type: {Type}, Value: {Value}", kvp.Key, kvp.Value?.GetType().Name, kvp.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al loggear contenido del documento {DocumentId}", doc.Id);
            }
        }
    }
}