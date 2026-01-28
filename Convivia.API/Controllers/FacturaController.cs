using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Application.Services;
using Convivia.Shared.Helpers;

namespace Convivia.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de facturas dentro de espacios.
    /// Puedes crear facturas y editarlass, así como gestionar sus imágenes asociadas de forma separada
    /// </summary>
    [ApiController]
    [Route("api/espacio/{espacioId}/factura")]
    public class FacturaController : ControllerBase
    {
        private readonly FacturaService _service;

        public FacturaController(FacturaService service)
        {
            _service = service;
        }

        /// <summary>
        /// Crea una nueva factura en el espacio especificado.
        /// </summary>
        /// <param name="espacioId">ID del espacio donde se creará la factura</param>
        /// <param name="model">
        /// Datos de la factura a crear
        /// pagoMediano no es obligatioria, se puede autoCalcular
        /// </param>
        [HttpPost]
        public async Task<IActionResult> Create(string espacioId, [FromBody] CreateFacturaDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");
            if (model == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(model.Nombre)) return BadRequest("Nombre es requerido.");
            if (model.Precio < 0) return BadRequest("Precio no puede ser negativo.");

            var created = await _service.CrearFacturaAsync(espacioId, model, ct);
            return CreatedAtAction(nameof(GetById), new { espacioId, id = created.Id }, created);
        }

        /// <summary>
        /// Obtiene una factura específica por su ID.
        /// </summary>
        /// <param name="espacioId">ID del espacio</param>
        /// <param name="id">ID de la factura</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string espacioId, string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var factura = await _service.ObtenerFacturaAsync(espacioId, id, ct);
            if (factura == null) return NotFound();
            return Ok(factura);
        }

        /// <summary>
        /// Obtiene todas las facturas de un espacio.
        /// </summary>
        /// <param name="espacioId">ID del espacio</param>
        [HttpGet]
        public async Task<IActionResult> GetAll(string espacioId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");

            var list = await _service.ListarTodasAsync(espacioId, ct);
            return Ok(list);
        }

        /// <summary>
        /// Obtiene todas las facturas de un espacio creadas por un usuario específico.
        /// </summary>
        /// <param name="espacioId">ID del espacio</param>
        /// <param name="creadorId">ID del usuario creador (UsuarioEspacioId)</param>
        [HttpGet("creador/{creadorId}")]
        public async Task<IActionResult> GetByCreador(string espacioId, string creadorId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");
            if (string.IsNullOrWhiteSpace(creadorId)) return BadRequest("CreadorId es requerido.");

            var list = await _service.ListarPorCreadorAsync(espacioId, creadorId, ct);
            return Ok(list);
        }

        /// <summary>
        /// Obtiene todas las facturas de un espacio donde un usuario es deudor.
        /// </summary>
        /// <param name="espacioId">ID del espacio</param>
        /// <param name="deudorId">ID del usuario deudor (UsuarioEspacioId)</param>
        [HttpGet("deudor/{deudorId}")]
        public async Task<IActionResult> GetByDeudor(string espacioId, string deudorId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");
            if (string.IsNullOrWhiteSpace(deudorId)) return BadRequest("DeudorId es requerido.");

            var list = await _service.ListarPorDeudorAsync(espacioId, deudorId, ct);
            return Ok(list);
        }

        /// <summary>
        ///     Actualiza completamente una factura (overwrite). Reemplaza todos los campos.
        /// </summary>
        /// <param name="espacioId">ID del espacio</param>
        /// <param name="id">ID de la factura a actualizar</param>
        /// <param name="model">
        ///     Nuevos datos de la factura
        /// </param>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOverwrite(string espacioId, string id, [FromBody] UpdateFacturaDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarFacturaCompletaAsync(espacioId, id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Actualiza una factura fusionando los campos enviados con los existentes (merge).
        /// </summary>
        /// <param name="espacioId">ID del espacio</param>
        /// <param name="id">ID de la factura a actualizar</param>
        /// <param name="model">Campos a actualizar (solo los enviados se modifican)</param>
        [HttpPut("{id}/merge")]
        public async Task<IActionResult> PutMerge(string espacioId, string id, [FromBody] UpdateFacturaDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarFacturaMergeAsync(espacioId, id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Actualiza parcialmente una factura. Solo actualiza los campos enviados.
        /// </summary>
        /// <param name="espacioId">ID del espacio</param>
        /// <param name="id">ID de la factura a actualizar</param>
        /// <param name="model">Campos a actualizar (los no enviados no se modifican)</param>
        /// <returns>La factura actualizada</returns>
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(string espacioId, string id, [FromBody] UpdateFacturaDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarFacturaParcialAsync(espacioId, id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Elimina una factura y su imagen asociada (si existe).
        /// </summary>
        /// <param name="espacioId">ID del espacio</param>
        /// <param name="id">ID de la factura a eliminar</param>
        /// <returns>Sin contenido si se eliminó correctamente</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string espacioId, string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var resultat = await _service.EliminarFacturaAsync(espacioId, id, ct);
            return resultat ? NoContent() : NotFound();
        }

        // === ENDPOINTS DE GESTIÓN DE IMÁGENES ===

        /// <summary>
        /// Obtiene la imagen asociada a una factura.
        /// </summary>
        /// <param name="espacioId">ID del espacio</param>
        /// <param name="id">ID de la factura</param>
        /// <returns>Archivo de imagen en formato JPEG</returns>
        [HttpGet("{id}/imagen")]
        [Produces("image/jpeg", "image/png", "image/gif")]
        public async Task<IActionResult> GetImagen(string espacioId, string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var imagen = await _service.ObtenerImagenAsync(espacioId, id, ct);
            if (imagen == null || imagen.Length == 0)
                return NotFound("La factura no tiene imagen.");

            return File(imagen, "image/jpeg");
        }

        /// <summary>
        /// Sube una imagen para una factura existente.
        /// </summary>
        /// <param name="espacioId">ID del espacio</param>
        /// <param name="id">ID de la factura</param>
        /// <param name="imagen">Archivo de imagen (máximo 0.5 MiB)</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Confirmación de la operación</returns>
        /// <remarks>
        /// Tamaño máximo: 0.5 MiB (524,288 bytes). Solo se aceptan archivos de tipo image/*.
        /// Si la imagen supera el tamaño máximo, se redimensionará automáticamente para adaptarse.
        /// </remarks>
        [HttpPost("{id}/imagen")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImagen(string espacioId, string id, IFormFile imagen, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("Id es requerido.");
            if (imagen == null || imagen.Length == 0)
                return BadRequest("No se envió ninguna imagen.");

            if (!imagen.ContentType.StartsWith("image/"))
                return BadRequest("El archivo debe ser una imagen.");

            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                await imagen.CopyToAsync(ms, ct);
                bytes = ms.ToArray();
            }

            // Optimizar la imagen si supera el tamaño máximo
            var originalSize = bytes.Length;
            bytes = ImageHelper.OptimizeImageIfNeeded(bytes);
            var wasOptimized = originalSize != bytes.Length;

            var result = await _service.ActualizarImagenAsync(espacioId, id, bytes, ct);
            if (!result) return NotFound("Factura no encontrada.");

            var message = wasOptimized 
                ? $"Imagen subida y optimizada correctamente. Tamaño original: {originalSize} bytes, tamaño final: {bytes.Length} bytes."
                : "Imagen subida correctamente.";

            return Ok(new { success = true, message, wasOptimized, originalSize, finalSize = bytes.Length });
        }

        /// <summary>
        /// Actualiza/reemplaza la imagen de una factura existente.
        /// </summary>
        /// <param name="espacioId">ID del espacio</param>
        /// <param name="id">ID de la factura</param>
        /// <param name="imagen">Archivo de imagen (máximo 0.5 MiB)</param>
        /// <remarks>
        /// Tamaño máximo: 0.5 MiB (524,288 bytes). Solo se aceptan archivos de tipo image/*.
        /// Si la imagen supera el tamaño máximo, se redimensionará automáticamente para adaptarse.
        /// </remarks>
        [HttpPut("{id}/imagen")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateImagen(string espacioId, string id, IFormFile imagen, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("Id es requerido.");
            if (imagen == null || imagen.Length == 0)
                return BadRequest("No se envió ninguna imagen.");

            if (!imagen.ContentType.StartsWith("image/"))
                return BadRequest("El archivo debe ser una imagen.");

            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                await imagen.CopyToAsync(ms, ct);
                bytes = ms.ToArray();
            }

            // Optimizar la imagen si supera el tamaño máximo
            var originalSize = bytes.Length;
            bytes = ImageHelper.OptimizeImageIfNeeded(bytes);
            var wasOptimized = originalSize != bytes.Length;

            var result = await _service.ActualizarImagenAsync(espacioId, id, bytes, ct);
            if (!result) return NotFound("Factura no encontrada.");

            var message = wasOptimized 
                ? $"Imagen actualizada y optimizada correctamente. Tamaño original: {originalSize} bytes, tamaño final: {bytes.Length} bytes."
                : "Imagen actualizada correctamente.";

            return Ok(new { success = true, message, wasOptimized, originalSize, finalSize = bytes.Length });
        }

        /// <summary>
        /// Elimina la imagen de una factura (mantiene la factura).
        /// </summary>
        /// <param name="espacioId">ID del espacio</param>
        /// <param name="id">ID de la factura</param>
        /// <returns>Sin contenido si se eliminó correctamente</returns>
        [HttpDelete("{id}/imagen")]
        public async Task<IActionResult> DeleteImagen(string espacioId, string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var result = await _service.EliminarImagenAsync(espacioId, id, ct);
            return result ? NoContent() : NotFound();
        }
    }
}
