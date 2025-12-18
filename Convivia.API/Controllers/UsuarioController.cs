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

        // POST api/usuarios
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUsuarioDto model, CancellationToken ct)
        {
            if (model == null) return BadRequest();

          
            if (string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Nombre))
                
            {
                return BadRequest("email y nombre son requeridos");
            }

            var created = await _service.CrearUsuarioAsync(model, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.IdUsuario }, created);
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

        // DELETE api/invitaciones/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var removed = await _service.EliminarAsync(id, ct);
            if (!removed) return NotFound();
            return NoContent();
        }
    }
}