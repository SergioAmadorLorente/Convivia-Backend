using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using System.IO;
using System.Text.Json;
using Convivia.Infrastructure.Infraestructure;

namespace Convivia.Infrastructure.Infraestructure
{
    public static class FirebaseConfig
    {
        /// <summary>
        /// Inicializa la instancia de FirebaseApp si no existe.
        /// Tambi�n establece GOOGLE_APPLICATION_CREDENTIALS para ADC cuando aplica.
        /// Soporta: archivo en disco, variable FIREBASE_CREDENTIALS_JSON, o ADC.
        /// </summary>
        public static void InitializeFirebase()
        {
            if (FirebaseApp.DefaultInstance != null)
            {
                Console.WriteLine("[FirebaseConfig] FirebaseApp ya est� inicializado. Omitiendo.");
                return;
            }

            try
            {
                // 1. Determinar ruta de credenciales
                var credPath = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_PATH")
                    ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Firebase", "serviceAccount.json");

                Console.WriteLine($"[FirebaseConfig] Buscando credenciales en: {credPath}");
                Console.WriteLine($"[FirebaseConfig] BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");

                GoogleCredential credential = null;
                // 2. Intentar archivo si existe
                if (File.Exists(credPath))
                {
                    Console.WriteLine($"[FirebaseConfig] Archivo encontrado. Validando contenido...");
                    var fileJson = File.ReadAllText(credPath);
                    // Mostrar primeros caracteres para verificar que se est� leyendo correctamente
                    Console.WriteLine($"[FirebaseConfig] Primeros 100 caracteres del archivo: {fileJson.Substring(0, Math.Min(100, fileJson.Length))}");
                    using var doc = JsonDocument.Parse(fileJson);
                    var root = doc.RootElement;

                    var hasPk = root.TryGetProperty("private_key", out var pk) && !string.IsNullOrWhiteSpace(pk.GetString());
                    var hasCe = root.TryGetProperty("client_email", out var ce) && !string.IsNullOrWhiteSpace(ce.GetString());

                    if (hasPk && hasCe)
                    {
                        credential = GoogleCredential.FromFile(credPath);
                        // Establecer variable para ADC en el proceso (opcional pero �til)
                        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credPath, EnvironmentVariableTarget.Process);
                        Console.WriteLine("[FirebaseConfig] Credenciales v�lidas encontradas en archivo. GOOGLE_APPLICATION_CREDENTIALS establecido.");
                    }
                    else
                    {
                        Console.WriteLine("[FirebaseConfig] Archivo de credenciales presente pero incompleto (falta private_key o client_email).");
                    }
                }
                else
                {
                    Console.WriteLine($"[FirebaseConfig] No se encontr� el archivo en ruta esperada: {credPath}");
                }

                // 3. Si no obtuvimos credencial del archivo, probar variable de entorno con JSON completo
                if (credential == null)
                {
                    var envJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");
                    if (!string.IsNullOrWhiteSpace(envJson))
                    {
                        try
                        {
                            using var doc2 = JsonDocument.Parse(envJson);
                            var root2 = doc2.RootElement;
                            var hasPk2 = root2.TryGetProperty("private_key", out var pk2) && !string.IsNullOrWhiteSpace(pk2.GetString());
                            var hasCe2 = root2.TryGetProperty("client_email", out var ce2) && !string.IsNullOrWhiteSpace(ce2.GetString());

                            if (hasPk2 && hasCe2)
                            {
                                credential = GoogleCredential.FromJson(envJson);
                                Console.WriteLine("[FirebaseConfig] Credenciales v�lidas obtenidas desde FIREBASE_CREDENTIALS_JSON.");
                            }
                            else
                            {
                                Console.WriteLine("[FirebaseConfig] FIREBASE_CREDENTIALS_JSON est� presente pero no contiene private_key/client_email.");
                            }
                        }
                        catch (JsonException je)
                        {
                            Console.WriteLine($"[FirebaseConfig] FIREBASE_CREDENTIALS_JSON no es JSON v�lido: {je.Message}");
                        }
                    }
                }

                // 4. Si todav�a no hay credencial, intentar ADC (p. ej. en GCP, Cloud Run, etc.)
                if (credential == null)
                {
                    try
                    {
                        credential = GoogleCredential.GetApplicationDefault();
                        Console.WriteLine("[FirebaseConfig] Usando Application Default Credentials (ADC).");
                    }
                    catch (Exception)
                    {
                        // No hacemos nada aqu�: entraremos a la comprobaci�n final y lanzaremos error informativo.
                        credential = null;
                    }
                }

                // 5. Si no hay credencial v�lida, lanzar con instrucci�n clara
                if (credential == null)
                {
                    throw new InvalidOperationException(
                        "No se encontraron credenciales v�lidas para Firebase. Suministra un Service Account completo. " +
                        "Opciones v�lidas:\n" +
                        " - Guardar el JSON completo en 'Firebase/serviceAccount.json' (private_key y client_email presentes), o\n" +
                        " - Establecer la variable de entorno FIREBASE_CREDENTIALS_PATH apuntando a un archivo JSON v�lido, o\n" +
                        " - Establecer FIREBASE_CREDENTIALS_JSON con el contenido JSON del Service Account, o\n" +
                        " - Ejecutar en un entorno con ADC configuradas (p. ej. GCP)."
                    );
                }

                // 6. Crear instancia de FirebaseApp usando las credenciales resueltas
                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential
                });
                Console.WriteLine("[FirebaseConfig] FirebaseApp inicializado correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FirebaseConfig] Error detallado: {ex}");
                throw new InvalidOperationException("Error al inicializar Firebase.", ex);
            }
        }
    }
}