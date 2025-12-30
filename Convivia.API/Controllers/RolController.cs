using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Application.Services;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolController : ControllerBase
    {
        private readonly RolService _service;

        public RolController(RolService service)
        {
            _service = service;
        }

        // POST api/rol
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Create([FromBody] CreateRolDto model, CancellationToken ct)
        {
            if (model == null) return BadRequest("El modelo no puede ser nulo.");

            try
            {
                var created = await _service.CrearRolAsync(model, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET api/rol/{id}
        [HttpGet("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("ID es requerido.");

            var rol = await _service.ObtenerRolAsync(id, ct);
            if (rol == null) return NotFound();
            return Ok(rol);
        }

        // GET api/rol
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _service.ListarTodasAsync(ct);
            return Ok(list);
        }

        // GET api/rol/nombres-validos
        [HttpGet("nombres-validos")]
        public IActionResult GetNombresValidos()
        {
            return Ok(new
            {
                nombres = Enum.GetNames<TipoRol>(),
                descripcion = new
                {
                    Usuario = "Puede crear y editar tareas, y asignarse tareas",
                    Admin = "Tiene todos los permisos disponibles",
                    Moderador = "Puede crear, editar y eliminar tareas, asignar tareas y gestionar usuarios"
                }
            });
        }

        // PUT api/rol/{id}
        [HttpPut("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateRolDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("ID es requerido.");
            if (model == null) return BadRequest("El modelo no puede ser nulo.");

            try
            {
                var updated = await _service.ActualizarRolCompletoAsync(id, model, ct);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE api/rol/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("ID es requerido.");

            try
            {
                var removed = await _service.EliminarRolAsync(id, ct);
                if (!removed) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }
    }
}
