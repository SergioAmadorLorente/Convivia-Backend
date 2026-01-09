using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.Services;
using Microsoft.Extensions.Logging;

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

            // If collection contains '/', treat as path to subcollection
            if (collection.Contains("/"))
            {
                var parts = collection.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                // pattern: parentCollection/parentId/subcollection
                if (parts.Length >= 3)
                {
                    var parentCollection = parts[0];
                    var parentId = parts[1];
                    var subcollection = parts[2];
                    await _db.Collection(parentCollection).Document(parentId).Collection(subcollection).Document(id).SetAsync(entity, cancellationToken: cancellationToken);
                    return;
                }
            }

            await _db.Collection(collection).Document(id).SetAsync(entity, cancellationToken: cancellationToken);
        }

        public async Task<string> AddAsync<T>(string collection, T entity, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("collection required", nameof(collection));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            // Try read existing Id property from entity
            string? existingId = null;
            try
            {
                var prop = typeof(T).GetProperty("Id");
                if (prop != null && prop.CanRead)
                {
                    var val = prop.GetValue(entity) as string;
                    if (!string.IsNullOrWhiteSpace(val)) existingId = val;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not read Id property from entity of type {TypeName}", typeof(T).FullName);
            }

            if (collection.Contains("/"))
            {
                var parts = collection.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    var parentCollection = parts[0];
                    var parentId = parts[1];
                    var subcollection = parts[2];

                    if (!string.IsNullOrWhiteSpace(existingId))
                    {
                        // Use provided id to create document with same name
                        await _db.Collection(parentCollection).Document(parentId).Collection(subcollection).Document(existingId).SetAsync(entity, cancellationToken: cancellationToken);
                        return existingId;
                    }

                    var docRef = await _db.Collection(parentCollection).Document(parentId).Collection(subcollection).AddAsync(entity, cancellationToken: cancellationToken);
                    SetIdIfPossible(entity, docRef.Id);
                    return docRef.Id;
                }
            }

            if (!string.IsNullOrWhiteSpace(existingId))
            {
                await _db.Collection(collection).Document(existingId).SetAsync(entity, cancellationToken: cancellationToken);
                return existingId;
            }

            var newDoc = await _db.Collection(collection).AddAsync(entity, cancellationToken: cancellationToken);
            SetIdIfPossible(entity, newDoc.Id);
            return newDoc.Id;
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

            if (collection.Contains("/"))
            {
                var parts = collection.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    var parentCollection = parts[0];
                    var parentId = parts[1];
                    var subcollection = parts[2];
                    var snap = await _db.Collection(parentCollection).Document(parentId).Collection(subcollection).Document(id).GetSnapshotAsync(cancellationToken);
                    if (!snap.Exists) return null;
                    var entity = snap.ConvertTo<T>();
                    SetIdIfPossible(entity, snap.Id);
                    return entity;
                }
            }

            var snapTop = await _db.Collection(collection).Document(id).GetSnapshotAsync(cancellationToken);
            if (!snapTop.Exists) return null;
            var top = snapTop.ConvertTo<T>();
            SetIdIfPossible(top, snapTop.Id);
            return top;
        }

        // Update con opción merge (por defecto overwrite para compatibilidad)
        public async Task UpdateAsync<T>(string collection, string id, T entity, bool merge = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("collection required", nameof(collection));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id required", nameof(id));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var options = merge ? SetOptions.MergeAll : SetOptions.Overwrite;

            if (collection.Contains("/"))
            {
                var parts = collection.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    var parentCollection = parts[0];
                    var parentId = parts[1];
                    var subcollection = parts[2];
                    await _db.Collection(parentCollection).Document(parentId).Collection(subcollection).Document(id).SetAsync(entity, options, cancellationToken: cancellationToken);
                    return;
                }
            }

            await _db.Collection(collection).Document(id).SetAsync(entity, options, cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(string collection, string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("collection required", nameof(collection));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id required", nameof(id));
            if (updates == null) throw new ArgumentNullException(nameof(updates));
            if (updates.Count == 0) return;

            if (collection.Contains("/"))
            {
                var parts = collection.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    var parentCollection = parts[0];
                    var parentId = parts[1];
                    var subcollection = parts[2];
                    var docRef = _db.Collection(parentCollection).Document(parentId).Collection(subcollection).Document(id);
                    if (useSetMerge)
                        await docRef.SetAsync(updates, SetOptions.MergeAll, cancellationToken);
                    else
                        await docRef.UpdateAsync(updates, null, cancellationToken);
                    return;
                }
            }

            var doc = _db.Collection(collection).Document(id);
            if (useSetMerge)
                await doc.SetAsync(updates, SetOptions.MergeAll, cancellationToken);
            else
                await doc.UpdateAsync(updates, null ,cancellationToken);
        }

        public async Task DeleteAsync(string collection, string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("collection required", nameof(collection));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id required", nameof(id));

            if (collection.Contains("/"))
            {
                var parts = collection.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    var parentCollection = parts[0];
                    var parentId = parts[1];
                    var subcollection = parts[2];
                    await _db.Collection(parentCollection).Document(parentId).Collection(subcollection).Document(id).DeleteAsync(cancellationToken: cancellationToken);
                    return;
                }
            }

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

            Query q;
            if (collection.Contains("/"))
            {
                var parts = collection.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                // Support collection group pattern like 'plantillatareas/*/tareas'
                if (parts.Length >= 1 && parts[0].EndsWith("*") == false && parts.Length == 1)
                {
                    q = _db.Collection(parts[0]);
                }
                else if (collection.Contains("*/"))
                {
                    // pattern plantillatareas/*/tareas not directly supported by Collection(), use CollectionGroup
                    var sub = parts.Last();
                    q = _db.CollectionGroup(sub);
                }
                else
                {
                    q = _db.Collection(parts[0]);
                }
            }
            else
            {
                q = _db.Collection(collection);
            }

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
                    LogDocumentContents(doc);
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
                    LogDocumentContents(doc);
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
                var type = typeof(T);
                // Buscar propiedades por nombre común
                var candidates = new[] { "Id", "IdFactura", "Id_Factura", "IdFactura" };
                var prop = candidates
                    .Select(name => type.GetProperty(name))
                    .FirstOrDefault(p => p != null && p.CanWrite);

                // Si no encontramos por nombre exacto, buscar cualquier propiedad que contenga "id" (case-insensitive)
                if (prop == null)
                {
                    prop = type.GetProperties()
                               .FirstOrDefault(p => p.CanWrite && p.Name.IndexOf("id", StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (prop != null)
                {
                    // Convertir si la propiedad no es string
                    if (prop.PropertyType == typeof(string))
                    {
                        prop.SetValue(entity, id);
                    }
                    else
                    {
                        // intentar convertir a tipo destino si es posible
                        var converted = Convert.ChangeType(id, prop.PropertyType);
                        prop.SetValue(entity, converted);
                    }
                }
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
        public async Task<IEnumerable<T>> QueryArrayContainsAsync<T>(
    string collection,
    string field,
    object value)
        {
            var query = _db.Collection(collection)
                                    .WhereArrayContains(field, value);

            var snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents
                           .Select(doc => doc.ConvertTo<T>())
                           .ToList();
        }
    }
}