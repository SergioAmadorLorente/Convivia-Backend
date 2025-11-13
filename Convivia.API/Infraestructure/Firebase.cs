using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using System.IO;

namespace AuthApiDemo.Infraestructure
{
    public static class FirebaseConfig
    {
        /// <summary>
        /// Inicializa la instancia de FirebaseApp si no existe.
        /// También establece GOOGLE_APPLICATION_CREDENTIALS para ADC.
        /// </summary>
        /// 
        public static void InitializeFirebase()
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                try
                {
                    // 1. Determinar ruta de credenciales
                    var credPath = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_PATH")
                        ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Firebase", "serviceAccount.json");

                    // 2. Log de depuración: ruta que se está usando
                    Console.WriteLine($"[FirebaseConfig] Buscando credenciales en: {credPath}");

                    // 3. Verificar existencia
                    if (!File.Exists(credPath))
                        throw new FileNotFoundException($"No se encontró el archivo de credenciales: {credPath}");

                    // 4. Crear instancia de FirebaseApp
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(credPath)
                    });
                    Console.WriteLine("[FirebaseConfig] FirebaseApp inicializado correctamente.");

                    // 5. Establecer variable de entorno para Application Default Credentials
                    Environment.SetEnvironmentVariable(
                        "GOOGLE_APPLICATION_CREDENTIALS",
                        credPath,
                        EnvironmentVariableTarget.Process
                    );
                    Console.WriteLine("[FirebaseConfig] Variable GOOGLE_APPLICATION_CREDENTIALS establecida en el proceso.");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Error al inicializar Firebase.", ex);
                }
            }
        }
    }
}