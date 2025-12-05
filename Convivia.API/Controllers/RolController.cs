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
        private static readonly string[] NombresValidos = { "Usuario", "Admin" };

        public RolController(RolService service)
        {
            _service = service;
        }

        // POST api/rol
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRolDto model, CancellationToken ct)
        {
            if (model == null) return BadRequest("El modelo no puede ser nulo.");
            if (string.IsNullOrWhiteSpace(model.Nombre))
            {
                return BadRequest("Nombre es requerido.");
            }

            // Validar que el nombre sea válido
            if (!EsNombreValido(model.Nombre))
            {
                return BadRequest(new
                {
                    error = $"Nombre '{model.Nombre}' no válido.",
                    nombresPermitidos = NombresValidos
                });
            }

            try
            {
                var id = await _service.CrearAsync(model, ct);
                return CreatedAtAction(nameof(GetById), new { id }, new { id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET api/rol/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("ID es requerido.");

            var rol = await _service.ObtenerPorIdAsync(id, ct);
            if (rol == null) return NotFound();
            return Ok(rol);
        }

        // GET api/rol
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _service.ObtenerTodosAsync(ct);
            return Ok(list);
        }

        // GET api/rol/nombres-validos
        [HttpGet("nombres-validos")]
        public IActionResult GetNombresValidos()
        {
            return Ok(new
            {
                nombres = NombresValidos,
                descripcion = new
                {
                    Usuario = "Puede crear y editar tareas, y asignarse tareas",
                    Admin = "Tiene todos los permisos disponibles"
                }
            });
        }

        // GET api/rol/por-nombre/{nombre}
        [HttpGet("nombre/{nombre}")]
        public async Task<IActionResult> GetByNombre(string nombre, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return BadRequest("Nombre es requerido.");

            // Validar que el nombre sea válido
            if (!EsNombreValido(nombre))
            {
                return BadRequest(new
                {
                    error = $"Nombre '{nombre}' no válido.",
                    nombresPermitidos = NombresValidos
                });
            }

            try
            {
                var rol = await _service.ObtenerPorNombreAsync(nombre, ct);
                if (rol == null) return NotFound();
                return Ok(rol);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT api/rol/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateRolDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("ID es requerido.");
            if (model == null) return BadRequest("El modelo no puede ser nulo.");

            // Validar nombre si se está actualizando
            if (model.Nombre != null && !EsNombreValido(model.Nombre))
            {
                return BadRequest(new
                {
                    error = $"Nombre '{model.Nombre}' no válido.",
                    nombresPermitidos = NombresValidos
                });
            }

            try
            {
                var updated = await _service.ActualizarAsync(id, model, ct);
                if (!updated) return NotFound();
                return NoContent();
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

            var removed = await _service.EliminarAsync(id, ct);
            if (!removed) return NotFound();
            return NoContent();
        }

        private static bool EsNombreValido(string nombre)
        {
            return !string.IsNullOrWhiteSpace(nombre) && 
                   NombresValidos.Contains(nombre, StringComparer.OrdinalIgnoreCase);
        }
    }
}
