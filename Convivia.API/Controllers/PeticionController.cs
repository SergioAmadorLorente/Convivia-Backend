using Convivia.Shared.DTOs;
using Convivia.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PeticionController : ControllerBase
    {
        private readonly PeticionService _service;

        public PeticionController(PeticionService service)
        {
            _service = service;
        }

        // POST api/peticion
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePeticionDto model, CancellationToken ct)
        {
            if (model == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(model.Mensaje)) return BadRequest("Mensaje es requerido.");
            if (string.IsNullOrWhiteSpace(model.IdSolicitante)) return BadRequest("Id solicitante es requerido.");
            if (string.IsNullOrWhiteSpace(model.IdEspacio)) return BadRequest("Id espacio es requerido.");

            var created = await _service.CrearPeticionAsync(model, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }


        // GET api/peticion/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var peticion = await _service.ObtenerPeticionAsync(id, ct);
            if (peticion == null) return NotFound();
            return Ok(peticion);
        }

        // GET api/peticion
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _service.ListarTodasAsync(ct);
            return Ok(list);
        }

        // PUT api/peticion/{id}
        // Overwrite completo: reemplaza todo el documento en Firestore.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOverwrite(string id, [FromBody] UpdatePeticionDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarPeticionCompletaAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // PUT api/peticion/{id}/merge
        // Merge explícito: fusiona los campos del DTO con el documento existente.
        [HttpPut("{id}/merge")]
        public async Task<IActionResult> PutMerge(string id, [FromBody] UpdatePeticionDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarPeticionMergeAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // PATCH api/peticion/{id}
        // Parcial: actualiza solo los campos enviados (IDictionary -> Update parcial en Firestore).
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(string id, [FromBody] UpdatePeticionDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarPeticionParcialAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }


        // DELETE api/peticion/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var resultat = await _service.EliminarPeticionAsync(id, ct);
            return resultat ? NoContent() : NotFound();
        }
    }

}
