using Convivia.Application.Services;
using Convivia.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Convivia.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioEspacioController : ControllerBase
    {
        private readonly UsuarioEspacioService _service;

        public UsuarioEspacioController(UsuarioEspacioService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        // POST api/usuarioEspacio
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUsuarioEspacioDto model, CancellationToken ct)
        {
            if (model == null) return BadRequest();
            if (model.Ausente == null) throw new ArgumentNullException(nameof(model.Ausente));
            if (model.Karma < 0) throw new ArgumentOutOfRangeException(nameof(model.Karma), "Karma no puede ser negativo");
            if (string.IsNullOrWhiteSpace(model.Rol)) throw new ArgumentException("Rol no puede estar vacío", nameof(model.Rol));
            if (string.IsNullOrWhiteSpace(model.EspacioId)) throw new ArgumentException("EspacioId no puede estar vacío", nameof(model.EspacioId));
            if (string.IsNullOrWhiteSpace(model.UsuarioId)) throw new ArgumentException("UsuarioId no puede estar vacío", nameof(model.UsuarioId));
            if (model.TareasId == null) throw new ArgumentNullException(nameof(model.TareasId));
            if (string.IsNullOrWhiteSpace(model.PermisoId)) throw new ArgumentException("PermisoId no puede estar vacío", nameof(model.PermisoId));

            var created = await _service.CrearUsuarioEspacioAsync(model, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // GET api/usuarioEspacio/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var usuarioEspacio = await _service.ObtenerUsuarioEspacioAsync(id, ct);
            if (usuarioEspacio == null) return NotFound();
            return Ok(usuarioEspacio);
        }

        // GET: api/usuarioespacio/espacio/{espacioId}
        [HttpGet("espacio/{espacioId}")]
        public async Task<ActionResult<IEnumerable<UsuarioEspacioDto>>> ObtenerPorEspacio(string espacioId, CancellationToken ct)
        {
            var result = await _service.ObtenerPorEspacioAsync(espacioId, ct);
            return Ok(result);
        }

        // GET: api/usuarioespacio/usuario/{usuarioId}
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<UsuarioEspacioDto>>> ObtenerPorUsuario(string usuarioId, CancellationToken ct)
        {
            var result = await _service.ObtenerPorUsuarioAsync(usuarioId, ct);
            return Ok(result);
        }

        // GET api/usuarioEspacio
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _service.ListarTodasAsync(ct);
            return Ok(list);
        }

        // PUT api/usuarioEspacio/{id}
        // Overwrite completo: reemplaza todo el documento en Firestore.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOverwrite(string id, [FromBody] UpdateUsuarioEspacioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarUsuarioEspacioCompletoAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // PUT api/usuarioEspacio/{id}/merge
        // Merge explícito: fusiona los campos del DTO con el documento existente.
        [HttpPut("{id}/merge")]
        public async Task<IActionResult> PutMerge(string id, [FromBody] UpdateUsuarioEspacioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarUsuarioEspacioMergeAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // PATCH api/usuarioEspacio/{id}
        // Parcial: actualiza solo los campos enviados (IDictionary -> Update parcial en Firestore).
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(string id, [FromBody] UpdateUsuarioEspacioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarUsuarioEspacioParcialAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // DELETE api/usuarioespacio/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var resultat = await _service.EliminarUsuarioEspacioAsync(id, ct);
            return resultat ? NoContent() : NotFound();
        }
    }
}
