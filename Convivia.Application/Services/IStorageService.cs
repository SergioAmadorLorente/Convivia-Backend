using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Application.Services
{
    /// <summary>
    /// Servicio abstracto para el almacenamiento y gestión de archivos de media (imágenes, documentos, etc.).
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Sube un archivo al almacenamiento (Firebase Storage o local) y devuelve la URL pública.
        /// </summary>
        /// <param name="fileStream">Stream de datos del archivo</param>
        /// <param name="fileName">Nombre del archivo con extensión</param>
        /// <param name="contentType">Tipo MIME del archivo (ej. image/jpeg, image/png)</param>
        /// <param name="folder">Carpeta destino (ej. perfiles)</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>URL pública para acceder al archivo guardado</returns>
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder = "perfiles", CancellationToken ct = default);

        /// <summary>
        /// Elimina un archivo del almacenamiento basándose en su URL o ruta.
        /// </summary>
        /// <param name="fileUrl">URL completa o ruta del archivo a eliminar</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>True si se eliminó con éxito, False en caso contrario</returns>
        Task<bool> DeleteFileAsync(string fileUrl, CancellationToken ct = default);
    }
}
