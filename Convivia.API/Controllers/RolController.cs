using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Application.Services;

namespace Convivia.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de roles del sistema.
    /// Define los diferentes tipos de roles disponibles: Usuario, Admin y Moderador.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RolController : ControllerBase
    {
        private readonly RolService _service;

        public RolController(RolService service)
        {
            _service = service;
        }

        /// <summary>
        /// Crea un nuevo rol.
        /// </summary>
        /// <param name="model">Datos del rol a crear</param>
        /// <param name="ct">Token de cancelación</param>
        /// <remarks>Este endpoint está oculto de la documentación de la API</remarks>
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

        /// <summary>
        /// Obtiene un rol específico por su ID.
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <param name="ct">Token de cancelación</param>
        /// <remarks>Este endpoint está oculto de la documentación de la API</remarks>
        [HttpGet("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("ID es requerido.");

            var rol = await _service.ObtenerRolAsync(id, ct);
            if (rol == null) return NotFound();
            return Ok(rol);
        }

        /// <summary>
        /// Obtiene todos los roles disponibles en el sistema.
        /// </summary>
        /// <param name="ct">Token de cancelación</param>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _service.ListarTodasAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Obtiene los nombres válidos de roles disponibles con sus descripciones.
        /// </summary>
        /// <returns>Nombres de roles y sus descripciones detalladas</returns>
        /// <remarks>
        /// Los roles disponibles son:
        /// - Usuario: Puede crear y editar tareas, y asignarse tareas
        /// - Admin: Tiene todos los permisos disponibles
        /// - Moderador: Puede crear, editar y eliminar tareas, asignar tareas y gestionar usuarios
        /// </remarks>
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

        /// <summary>
        /// Actualiza un rol existente.
        /// </summary>
        /// <param name="id">ID del rol a actualizar</param>
        /// <param name="model">Nuevos datos del rol</param>
        /// <param name="ct">Token de cancelación</param>
        /// <remarks>Este endpoint está oculto de la documentación de la API</remarks>
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

        /// <summary>
        /// Elimina un rol del sistema.
        /// </summary>
        /// <param name="id">ID del rol a eliminar</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Sin contenido si se eliminó correctamente, o error de conflicto si hay restricciones</returns>
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
