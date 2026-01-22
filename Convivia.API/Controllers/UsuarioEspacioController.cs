using Convivia.Application.Services;
using Convivia.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Convivia.Api.Controllers
{
    /// <summary>
    /// Controlador para la gestión de la relación entre usuarios y espacios.
    /// Gestiona roles, permisos, karma y tareas asignadas a usuarios dentro de un espacio específico.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioEspacioController : ControllerBase
    {
        private readonly UsuarioEspacioService _service;

        public UsuarioEspacioController(UsuarioEspacioService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Crea una nueva relación usuario-espacio.
        /// </summary>
        /// <param name="model">Datos de la relación a crear (rol, karma, permisos, tareas)</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>La relación usuario-espacio creada</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUsuarioEspacioDto model, CancellationToken ct)
        {
            try
            {
                var created = await _service.CrearUsuarioEspacioAsync(model, ct);
                if (created == null)
                    return BadRequest("No se pudo crear el UsuarioEspacio.");
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        /// <summary>
        /// Obtiene una relación usuario-espacio específica por su ID.
        /// </summary>
        /// <param name="id">ID de la relación usuario-espacio</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>La relación usuario-espacio solicitada</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var usuarioEspacio = await _service.ObtenerUsuarioEspacioAsync(id, ct);
            if (usuarioEspacio == null) return NotFound();
            return Ok(usuarioEspacio);
        }

        /// <summary>
        /// Obtiene todos los usuarios que pertenecen a un espacio específico.
        /// </summary>
        /// <param name="espacioId">ID del espacio</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-espacio del espacio</returns>
        [HttpGet("espacio/{espacioId}")]
        public async Task<ActionResult<IEnumerable<UsuarioEspacioDto>>> ObtenerPorEspacio(string espacioId, CancellationToken ct)
        {
            var result = await _service.ObtenerPorEspacioAsync(espacioId, ct);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene todos los espacios a los que pertenece un usuario específico.
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-espacio del usuario</returns>
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<UsuarioEspacioDto>>> ObtenerPorUsuario(string usuarioId, CancellationToken ct)
        {
            var result = await _service.ObtenerPorUsuarioAsync(usuarioId, ct);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene todas las relaciones usuario-espacio del sistema.
        /// </summary>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Lista de todas las relaciones usuario-espacio</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _service.ListarTodasAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Actualiza completamente una relación usuario-espacio (overwrite). Reemplaza todos los campos.
        /// </summary>
        /// <param name="id">ID de la relación usuario-espacio a actualizar</param>
        /// <param name="model">Nuevos datos completos de la relación</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>La relación usuario-espacio actualizada</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOverwrite(string id, [FromBody] UpdateUsuarioEspacioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarUsuarioEspacioCompletoAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Actualiza una relación usuario-espacio fusionando los campos enviados con los existentes (merge).
        /// </summary>
        /// <param name="id">ID de la relación usuario-espacio a actualizar</param>
        /// <param name="model">Campos a actualizar (solo los enviados se modifican)</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>La relación usuario-espacio actualizada</returns>
        [HttpPut("{id}/merge")]
        public async Task<IActionResult> PutMerge(string id, [FromBody] UpdateUsuarioEspacioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarUsuarioEspacioMergeAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Actualiza parcialmente una relación usuario-espacio. Solo actualiza los campos enviados.
        /// </summary>
        /// <param name="id">ID de la relación usuario-espacio a actualizar</param>
        /// <param name="model">Campos a actualizar (los no enviados no se modifican)</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>La relación usuario-espacio actualizada</returns>
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(string id, [FromBody] UpdateUsuarioEspacioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarUsuarioEspacioParcialAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Elimina una relación usuario-espacio.
        /// </summary>
        /// <param name="id">ID de la relación usuario-espacio a eliminar</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Sin contenido si se eliminó correctamente, o error de conflicto si hay restricciones</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            try
            {
                var resultat = await _service.EliminarUsuarioEspacioAsync(id, ct);
                return resultat ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
