using Google.Cloud.Firestore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApiDemo.Services
{
    public class FirebaseService : IFirebaseService
    {
        private readonly FirestoreDb _db;

        public FirebaseService(FirestoreDb firestoreDb)
        {
            _db = firestoreDb;
        }

        public async Task AddAsync<T>(string collection, string id, T entity)
        {
            await _db.Collection(collection).Document(id).SetAsync(entity);
        }

        public async Task<T?> GetAsync<T>(string collection, string id) where T : class
        {
            var snap = await _db.Collection(collection).Document(id).GetSnapshotAsync();
            if (!snap.Exists) return null;
            return snap.ConvertTo<T>();
        }

        public async Task UpdateAsync<T>(string collection, string id, T entity)
        {
            await _db.Collection(collection).Document(id).SetAsync(entity, SetOptions.Overwrite);
        }

        public async Task DeleteAsync(string collection, string id)
        {
            await _db.Collection(collection).Document(id).DeleteAsync();
        }

        public async Task<List<T>> QueryAsync<T>(string collection, string field, object value) where T : class
        {
            var snap = await _db.Collection(collection).WhereEqualTo(field, value).GetSnapshotAsync();
            return snap.Documents.Select(d => d.ConvertTo<T>()).ToList();
        }

        public async Task<List<T>> QueryMultipleConditionsAsync<T>(string collection, (string field, object val)[] conditions) where T : class
        {
            Query q = _db.Collection(collection);
            foreach (var c in conditions)
                q = q.WhereEqualTo(c.field, c.val);

            var snap = await q.GetSnapshotAsync();
            return snap.Documents.Select(d => d.ConvertTo<T>()).ToList();
        }

        public async Task<List<T>> GetAllAsync<T>(string collection) where T : class
        {
            var snapshot = await _db.Collection(collection).GetSnapshotAsync();
            var result = new List<T>();
            foreach (var doc in snapshot.Documents)
            {
                var entity = doc.ConvertTo<T>();
                if (entity != null)
                    result.Add(entity);
            }
            return result;
        }
    }
}