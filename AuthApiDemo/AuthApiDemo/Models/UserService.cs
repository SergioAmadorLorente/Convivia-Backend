using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;

namespace AuthApiDemo.Models
{
    public class UserService
    {
        private readonly ILogger<UserService> _logger;

        public UserService(ILogger<UserService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> ProbarConexionAsync()
        {
            try
            {
                FirestoreDb db = FirestoreDb.Create("convivia-9f62b");
                Console.WriteLine("Conectado a Firestore: " + db.ProjectId);

                DocumentReference docRef = db.Collection("usuarios").Document("usuario1");
                await docRef.SetAsync(new { Email = "prueba@correo.com", Password = "123456" });

                var snapshot = await db.Collection("usuarios").GetSnapshotAsync();
                foreach (var doc in snapshot.Documents)
                {
                    _logger.LogInformation($"Documento ID: {doc.Id}, Datos: {doc.ToDictionary()}");
                }
                return snapshot.Count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error de conexión a Firestore: {ex.Message}");
                return false;
            }
        }
    }
}
