using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Application.Services;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _service;

        public UsuarioController(UsuarioService service)
        {
            _service = service;
        }

        // POST api/usuario
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUsuarioDto model, CancellationToken ct)
        {
            if (model == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(model.Nombre)) return BadRequest("Nombre es requerido.");
            if (string.IsNullOrWhiteSpace(model.Email)) return BadRequest("Email es requerido.");
            if (string.IsNullOrWhiteSpace(model.Password)) return BadRequest("Password es requerido.");


            var created = await _service.CrearUsuarioAsync(model, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // GET api/usuarios/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var usuario = await _service.ObtenerUsuarioAsync(id, ct);
            if (usuario == null) return NotFound();
            return Ok(usuario);
        }
        // GET api/Usuario
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _service.ListarTodasAsync(ct);
            return Ok(list);
        }

        // PUT api/usuario
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOverwrite(string id, [FromBody] UpdateUsuarioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarUsuarioCompletoAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // PUT api/usuario/{id}/merge
        // Merge explícito: fusiona los campos del DTO con el documento existente.
        [HttpPut("{id}/merge")]
        public async Task<IActionResult> PutMerge(string id, [FromBody] UpdateUsuarioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarUsuarioMergeAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // PATCH api/usuario/{id}
        // Parcial: actualiza solo los campos enviados (IDictionary -> Update parcial en Firestore).
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(string id, [FromBody] UpdateUsuarioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarUsuarioCompletoAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        // DELETE api/Usuario/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var resultat = await _service.EliminarUsuarioAsync(id, ct);
            return resultat ? NoContent() : NotFound();
        }
    }
}