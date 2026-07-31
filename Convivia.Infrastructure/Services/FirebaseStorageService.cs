using Convivia.Application.Services;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Infrastructure.Services
{
    /// <summary>
    /// Servicio de almacenamiento en la nube mediante Firebase Storage / Google Cloud Storage,
    /// con respaldo automático a almacenamiento local (wwwroot/uploads) para entornos de desarrollo.
    /// </summary>
    public class FirebaseStorageService : IStorageService
    {
        private readonly IConfiguration _config;
        private readonly IHostEnvironment _env;
        private readonly ILogger<FirebaseStorageService> _logger;

        public FirebaseStorageService(
            IConfiguration config,
            IHostEnvironment env,
            ILogger<FirebaseStorageService> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder = "perfiles",
            CancellationToken ct = default)
        {
            if (fileStream == null || fileStream.Length == 0)
                throw new ArgumentException("El stream del archivo no puede estar vacío.", nameof(fileStream));

            var extension = Path.GetExtension(fileName);
            var safeFileName = $"{Guid.NewGuid():N}{extension}";
            var objectPath = $"{folder}/{safeFileName}";

            var projectId = _config["Firebase:ProjectId"]
                ?? Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID")
                ?? "convivia-862f2";

            var bucketName = _config["Firebase:StorageBucket"]
                ?? Environment.GetEnvironmentVariable("FIREBASE_STORAGE_BUCKET")
                ?? $"{projectId}.firebasestorage.app";

            if (!string.IsNullOrWhiteSpace(bucketName))
            {
                try
                {
                    _logger.LogInformation("Intentando subir archivo {ObjectPath} a Firebase Storage Bucket: {Bucket}", objectPath, bucketName);
                    
                    var storageClient = await StorageClient.CreateAsync();
                    
                    fileStream.Position = 0;
                    var uploadedObject = await storageClient.UploadObjectAsync(
                        bucket: bucketName,
                        objectName: objectPath,
                        contentType: contentType,
                        source: fileStream,
                        options: new UploadObjectOptions { PredefinedAcl = PredefinedObjectAcl.PublicRead },
                        cancellationToken: ct
                    );

                    // Formato de URL pública de Firebase Cloud Storage
                    var publicUrl = $"https://firebasestorage.googleapis.com/v0/b/{bucketName}/o/{Uri.EscapeDataString(objectPath)}?alt=media";
                    _logger.LogInformation("Archivo subido exitosamente a Firebase Storage: {Url}", publicUrl);
                    return publicUrl;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al subir archivo a Firebase Storage (Bucket: {Bucket}). Alternando a almacenamiento local de respaldo.", bucketName);
                }
            }
            else
            {
                _logger.LogInformation("Firebase StorageBucket no configurado. Utilizando almacenamiento local de respaldo.");
            }

            // Fallback a almacenamiento local en wwwroot/uploads/
            return await SaveFileLocallyAsync(fileStream, objectPath, ct);
        }

        private async Task<string> SaveFileLocallyAsync(Stream fileStream, string objectPath, CancellationToken ct)
        {
            var webRootPath = Path.Combine(_env.ContentRootPath, "wwwroot");

            var fullPath = Path.Combine(webRootPath, "uploads", objectPath.Replace('/', Path.DirectorySeparatorChar));
            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            fileStream.Position = 0;
            using (var destinationStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(destinationStream, ct);
            }

            // URL relativa para ser servida por UseStaticFiles
            var relativeUrl = $"/uploads/{objectPath.Replace('\\', '/')}";
            _logger.LogInformation("Archivo guardado localmente: {FullPath} -> URL: {RelativeUrl}", fullPath, relativeUrl);
            return relativeUrl;
        }

        public async Task<bool> DeleteFileAsync(string fileUrl, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return false;

            try
            {
                if (fileUrl.StartsWith("/uploads/") || fileUrl.Contains("/uploads/"))
                {
                    var webRootPath = Path.Combine(_env.ContentRootPath, "wwwroot");
                    var relativePath = fileUrl.Substring(fileUrl.IndexOf("/uploads/")).TrimStart('/');
                    var fullPath = Path.Combine(webRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        _logger.LogInformation("Archivo local eliminado: {FullPath}", fullPath);
                        return true;
                    }
                }
                else
                {
                    var bucketName = _config["Firebase:StorageBucket"]
                        ?? Environment.GetEnvironmentVariable("FIREBASE_STORAGE_BUCKET");

                    if (!string.IsNullOrWhiteSpace(bucketName))
                    {
                        var storageClient = await StorageClient.CreateAsync();
                        // Extraer object path de la URL de Firebase
                        var objectName = ExtractObjectNameFromFirebaseUrl(fileUrl);
                        if (!string.IsNullOrWhiteSpace(objectName))
                        {
                            await storageClient.DeleteObjectAsync(bucketName, objectName, cancellationToken: ct);
                            _logger.LogInformation("Objeto en Firebase Storage eliminado: {ObjectName}", objectName);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar archivo: {FileUrl}", fileUrl);
            }

            return false;
        }

        private static string? ExtractObjectNameFromFirebaseUrl(string fileUrl)
        {
            try
            {
                if (fileUrl.Contains("/o/"))
                {
                    var part = fileUrl.Split("/o/")[1];
                    var objectName = part.Split('?')[0];
                    return Uri.UnescapeDataString(objectName);
                }
            }
            catch
            {
                // Ignorar error de parsing
            }
            return null;
        }
    }
}
