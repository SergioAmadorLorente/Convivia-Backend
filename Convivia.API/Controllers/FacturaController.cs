using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Application.Services;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("api/espacio/{espacioId}/factura")]
    public class FacturaController : ControllerBase
    {
        private readonly FacturaService _service;

        public FacturaController(FacturaService service)
        {
            _service = service;
        }

        // POST api/espacio/{espacioId}/factura
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

        // GET api/espacio/{espacioId}/factura/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string espacioId, string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var factura = await _service.ObtenerFacturaAsync(espacioId, id, ct);
            if (factura == null) return NotFound();
            return Ok(factura);
        }

        // GET api/espacio/{espacioId}/factura
        [HttpGet]
        public async Task<IActionResult> GetAll(string espacioId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");

            var list = await _service.ListarTodasAsync(espacioId, ct);
            return Ok(list);
        }

        // PUT api/espacio/{espacioId}/factura/{id}
        // Overwrite completo: reemplaza todo el documento en Firestore.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOverwrite(string espacioId, string id, [FromBody] UpdateFacturaDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarFacturaCompletaAsync(espacioId, id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // PUT api/espacio/{espacioId}/factura/{id}/merge
        // Merge explícito: fusiona los campos del DTO con el documento existente.
        [HttpPut("{id}/merge")]
        public async Task<IActionResult> PutMerge(string espacioId, string id, [FromBody] UpdateFacturaDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarFacturaMergeAsync(espacioId, id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // PATCH api/espacio/{espacioId}/factura/{id}
        // Parcial: actualiza solo los campos enviados (IDictionary -> Update parcial en Firestore).
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(string espacioId, string id, [FromBody] UpdateFacturaDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarFacturaParcialAsync(espacioId, id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // DELETE api/espacio/{espacioId}/factura/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string espacioId, string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) return BadRequest("EspacioId es requerido.");
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var resultat = await _service.EliminarFacturaAsync(espacioId, id, ct);
            return resultat ? NoContent() : NotFound();
        }

        // === ENDPOINTS DE GESTIÓN DE IMÁGENES ===

        // GET api/espacio/{espacioId}/factura/{id}/imagen
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

        // POST api/espacio/{espacioId}/factura/{id}/imagen
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

            const long maxSizeBytes = 524288; // 0.5 MiB
            if (imagen.Length > maxSizeBytes)
                return BadRequest($"La imagen excede el tamaño máximo permitido de 0.5 MiB ({maxSizeBytes} bytes). Tamaño actual: {imagen.Length} bytes.");

            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                await imagen.CopyToAsync(ms, ct);
                bytes = ms.ToArray();
            }

            var result = await _service.ActualizarImagenAsync(espacioId, id, bytes, ct);
            if (!result) return NotFound("Factura no encontrada.");

            return Ok(new { success = true, message = "Imagen subida correctamente." });
        }

        // PUT api/espacio/{espacioId}/factura/{id}/imagen
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
            const long maxSizeBytes = 524288; // 0.5 MiB
            if (imagen.Length > maxSizeBytes)
                return BadRequest($"La imagen excede el tamaño máximo permitido de {maxSizeBytes} bytes. Tamaño actual: {imagen.Length} bytes.");

            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                await imagen.CopyToAsync(ms, ct);
                bytes = ms.ToArray();
            }

            var result = await _service.ActualizarImagenAsync(espacioId, id, bytes, ct);
            if (!result) return NotFound("Factura no encontrada.");

            return Ok(new { success = true, message = "Imagen actualizada correctamente." });
        }

        // DELETE api/espacio/{espacioId}/factura/{id}/imagen
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
