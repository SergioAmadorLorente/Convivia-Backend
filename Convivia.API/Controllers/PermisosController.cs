using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Application.Services;
using Convivia.Domain.Entities;

namespace Convivia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermisosController : ControllerBase
    {
        private readonly PermisoService _service;
        private static readonly string[] RolesValidos = { "Usuario", "Admin" };

        public PermisosController(PermisoService service)
        {
            _service = service;
        }

        // POST api/permisos
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePermisoDto model, CancellationToken ct)
        {
            if (model == null) return BadRequest("El modelo no puede ser nulo.");
            if (string.IsNullOrWhiteSpace(model.Rol))
            {
                return BadRequest("Rol es requerido.");
            }

            if (!EsRolValido(model.Rol))
            {
                return BadRequest(new
                {
                    error = $"Rol '{model.Rol}' no válido.",
                    rolesPermitidos = RolesValidos
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

        // GET api/permisos/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("ID es requerido.");

            var permiso = await _service.ObtenerPorIdAsync(id, ct);
            if (permiso == null) return NotFound();
            return Ok(permiso);
        }

        // GET api/permisos
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _service.ObtenerTodosAsync(ct);
            return Ok(list);
        }

        // GET api/permisos/rol/{rol}
        [HttpGet("rol/{rol}")]
        public async Task<IActionResult> GetByRol(string rol, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(rol)) return BadRequest("Rol es requerido.");

            if (!EsRolValido(rol))
            {
                return BadRequest(new
                {
                    error = $"Rol '{rol}' no válido.",
                    rolesPermitidos = RolesValidos
                });
            }

            try
            {
                var list = await _service.ObtenerPorRolAsync(rol, ct);
                return Ok(list);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT api/permisos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdatePermisoDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("ID es requerido.");
            if (model == null) return BadRequest("El modelo no puede ser nulo.");

            if (model.Rol != null && !EsRolValido(model.Rol))
            {
                return BadRequest(new
                {
                    error = $"Rol '{model.Rol}' no válido.",
                    rolesPermitidos = RolesValidos
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

        // DELETE api/permisos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("ID es requerido.");

            var removed = await _service.EliminarAsync(id, ct);
            if (!removed) return NotFound();
            return NoContent();
        }

        private static bool EsRolValido(string rol)
        {
            return !string.IsNullOrWhiteSpace(rol) && 
                   RolesValidos.Contains(rol, StringComparer.OrdinalIgnoreCase);
        }
    }
}
