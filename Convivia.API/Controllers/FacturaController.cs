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
    }
}
