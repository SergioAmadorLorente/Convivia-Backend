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

        public PermisosController(PermisoService service)
        {
            _service = service;
        }

        // POST api/permisos
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePermisoDto model, CancellationToken ct)
        {
            if (model == null) return BadRequest("El modelo no puede ser nulo.");

            try
            {
                var id = await _service.CrearPermisoAsync(model, ct);
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

            var permiso = await _service.ObtenerPermisoAsync(id, ct);
            if (permiso == null) return NotFound();
            return Ok(permiso);
        }

        // GET api/permisos
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _service.ListarTodasAsync(ct);
            return Ok(list);
        }

        // GET api/permisos/rol/{rol}
        [HttpGet("rol/{rol}")]
        public async Task<IActionResult> GetByRol(TipoRol rol, CancellationToken ct)
        {
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

        // PUT api/permiso/{id}
        // Overwrite completo: reemplaza todo el documento en Firestore.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOverwrite(string id, [FromBody] UpdatePermisoDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarPermisoCompletaAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // PUT api/permiso/{id}/merge
        // Merge explícito: fusiona los campos del DTO con el documento existente.
        [HttpPut("{id}/merge")]
        public async Task<IActionResult> PutMerge(string id, [FromBody] UpdatePermisoDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarPermisoMergeAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // PATCH api/permiso/{id}
        // Parcial: actualiza solo los campos enviados (IDictionary -> Update parcial en Firestore).
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(string id, [FromBody] UpdatePermisoDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarPermisoParcialAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }



        // DELETE api/permiso/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var resultat = await _service.EliminarPermisoAsync(id, ct);
            return resultat ? NoContent() : NotFound();
        }
    }
}
