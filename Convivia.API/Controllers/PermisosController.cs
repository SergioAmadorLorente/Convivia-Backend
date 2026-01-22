using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Application.Services;
using Convivia.Domain.Entities;

namespace Convivia.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de permisos del sistema.
    /// Define qué acciones puede realizar cada rol dentro de los espacios.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PermisosController : ControllerBase
    {
        private readonly PermisoService _service;

        public PermisosController(PermisoService service)
        {
            _service = service;
        }

        /// <summary>
        /// Crea un nuevo conjunto de permisos.
        /// </summary>
        /// <param name="model">Datos del permiso a crear</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>El ID del permiso creado</returns>
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

        /// <summary>
        /// Obtiene un conjunto de permisos específico por su ID.
        /// </summary>
        /// <param name="id">ID del permiso</param>
        /// <param name="ct">Token de cancelación</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("ID es requerido.");

            var permiso = await _service.ObtenerPermisoAsync(id, ct);
            if (permiso == null) return NotFound();
            return Ok(permiso);
        }

        /// <summary>
        /// Obtiene todos los conjuntos de permisos del sistema.
        /// </summary>
        /// <param name="ct">Token de cancelación</param>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _service.ListarTodasAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Obtiene los permisos asociados a un rol específico.
        /// </summary>
        /// <param name="rol">Tipo de rol (Usuario, Admin, Moderador)</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Lista de permisos del rol especificado</returns>
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

        /// <summary>
        /// Actualiza completamente un conjunto de permisos (overwrite). Reemplaza todos los campos.
        /// </summary>
        /// <param name="id">ID del permiso a actualizar</param>
        /// <param name="model">Nuevos datos completos del permiso</param>
        /// <param name="ct">Token de cancelación</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOverwrite(string id, [FromBody] UpdatePermisoDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarPermisoCompletaAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Actualiza un conjunto de permisos fusionando los campos enviados con los existentes (merge).
        /// </summary>
        /// <param name="id">ID del permiso a actualizar</param>
        /// <param name="model">Campos a actualizar (solo los enviados se modifican)</param>
        /// <param name="ct">Token de cancelación</param>
        [HttpPut("{id}/merge")]
        public async Task<IActionResult> PutMerge(string id, [FromBody] UpdatePermisoDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarPermisoMergeAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Actualiza parcialmente un conjunto de permisos. Solo actualiza los campos enviados.
        /// </summary>
        /// <param name="id">ID del permiso a actualizar</param>
        /// <param name="model">Campos a actualizar (los no enviados no se modifican)</param>
        /// <param name="ct">Token de cancelación</param>
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(string id, [FromBody] UpdatePermisoDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarPermisoParcialAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }



        /// <summary>
        /// Elimina un conjunto de permisos.
        /// </summary>
        /// <param name="id">ID del permiso a eliminar</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Sin contenido si se eliminó correctamente</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var resultat = await _service.EliminarPermisoAsync(id, ct);
            return resultat ? NoContent() : NotFound();
        }
    }
}
