using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using System.IO;

/// <summary>
/// Configuración y inicialización de la conexión con Firebase.
/// </summary>
public static class FirebaseConfig
{
    /// <summary>
    /// Inicializa la instancia de FirebaseApp si no existe.
    /// La ruta del archivo de credenciales debe configurarse correctamente.
    /// </summary>
    public static void InitializeFirebase()
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            try
            {
                var credPath = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_PATH")
                    ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Firebase", "serviceAccount.json");

                if (!File.Exists(credPath))
                    throw new FileNotFoundException($"No se encontró el archivo de credenciales: {credPath}");

                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(credPath)
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al inicializar Firebase.", ex);
            }
        }
    }
}


