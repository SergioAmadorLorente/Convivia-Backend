using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Application.Services;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EspacioController : ControllerBase
    {
        private readonly EspacioService _service;

        public EspacioController(EspacioService service)
        {
            _service = service;
        }

        // POST api/espacio
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEspacioDto model, CancellationToken ct)
        {
            if (model == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(model.Nombre)) return BadRequest("Nombre es requerido.");
            

            var created = await _service.CrearEspacioAsync(model, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // GET api/espacio/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var espacio = await _service.ObtenerEspacioAsync(id, ct);
            if (espacio == null) return NotFound();
            return Ok(espacio);
        }

        // GET api/espacio
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _service.ListarTodasAsync(ct);
            return Ok(list);
        }

        // GET api/espacio/por-direccion/{direccion}
        [HttpGet("por-direccion/{direccion}")]
        public async Task<IActionResult> GetByDireccion(string direccion, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(direccion)) return BadRequest();

            var list = await _service.ObtenerPorDireccionAsync(direccion, ct);
            return Ok(list);
        }

        // PUT api/espacio/{id}
        // Overwrite completo: reemplaza todo el documento en Firestore.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOverwrite(string id, [FromBody] UpdateEspacioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarEspacioCompletoAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // PUT api/espacio/{id}/merge
        // Merge explícito: fusiona los campos del DTO con el documento existente.
        [HttpPut("{id}/merge")]
        public async Task<IActionResult> PutMerge(string id, [FromBody] UpdateEspacioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarEspacioCompletoAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // PATCH api/espacio/{id}
        // Parcial: actualiza solo los campos enviados (IDictionary -> Update parcial en Firestore).
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(string id, [FromBody] UpdateEspacioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarEspacioCompletoAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // DELETE api/espacio/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var result = await _service.EliminarEspacioAsync(id, ct);
            return result ? NoContent() : NotFound();
        }
    }
}