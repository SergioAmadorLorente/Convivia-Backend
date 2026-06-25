using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Application.Services;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Convivia.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de espacios compartidos.
    /// Permite crear, consultar, actualizar y eliminar espacios, así como gestionar códigos de invitación.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EspacioController : ControllerBase
    {
        private readonly EspacioService _service;

        public EspacioController(EspacioService service)
        {
            _service = service;
        }

        /// <summary>
        /// Crea un nuevo espacio compartido.
        /// </summary>
        /// <param name="model">Datos del espacio a crear</param>
        /// <param name="ct">Token de cancelación</param>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEspacioDto model, CancellationToken ct)
        {
            if (model == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(model.Nombre)) return BadRequest("Nombre es requerido.");
            

            var created = await _service.CrearEspacioAsync(model, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Obtiene un espacio específico por su ID.
        /// </summary>
        /// <param name="id">ID del espacio</param>
        /// <param name="ct">Token de cancelación</param>
        [HttpGet("{id}", Name = nameof(GetById))]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var espacio = await _service.ObtenerEspacioAsync(id, ct);
            if (espacio == null) return NotFound();
            return Ok(espacio);
        }

        /// <summary>
        /// Obtiene todos los espacios del sistema.
        /// </summary>
        /// <param name="ct">Token de cancelación</param>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _service.ListarTodasAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Obtiene espacios filtrados por dirección.
        /// </summary>
        /// <param name="direccion">Dirección a buscar</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Lista de espacios que coinciden con la dirección</returns>
        [HttpGet("por-direccion/{direccion}")]
        public async Task<IActionResult> GetByDireccion(string direccion, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(direccion)) return BadRequest();

            var list = await _service.ObtenerPorDireccionAsync(direccion, ct);
            return Ok(list);
        }

        /// <summary>
        /// Actualiza completamente un espacio (overwrite). Reemplaza todos los campos.
        /// </summary>
        /// <param name="id">ID del espacio a actualizar</param>
        /// <param name="model">Nuevos datos completos del espacio</param>
        /// <param name="ct">Token de cancelación</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOverwrite(string id, [FromBody] UpdateEspacioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarEspacioCompletoAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Actualiza un espacio fusionando los campos enviados con los existentes (merge).
        /// </summary>
        /// <param name="id">ID del espacio a actualizar</param>
        /// <param name="model">Campos a actualizar (solo los enviados se modifican)</param>
        /// <param name="ct">Token de cancelación</param>
        [HttpPut("{id}/merge")]
        public async Task<IActionResult> PutMerge(string id, [FromBody] UpdateEspacioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarEspacioCompletoAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Actualiza parcialmente un espacio. Solo actualiza los campos enviados.
        /// </summary>
        /// <param name="id">ID del espacio a actualizar</param>
        /// <param name="model">Campos a actualizar (los no enviados no se modifican)</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Sin contenido si se eliminó correctamente, o error de conflicto si hay restricciones</returns>
        /// <remarks>
        /// Nota: Este método actualmente ejecuta una eliminación en lugar de una actualización parcial.
        /// </remarks>
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(string id, [FromBody] UpdateEspacioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

                var result = await _service.ActualizarEspacioParcialAsync(id, model, ct);
            if (result == null) NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Elimina un espacio.
        /// </summary>
        /// <param name="id">ID del espacio a eliminar</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Sin contenido si se eliminó correctamente</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var result = await _service.EliminarEspacioAsync(id, ct);
            return result ? NoContent() : NotFound();
        }

        /// <summary>
        /// Genera o recupera el código de invitación de un espacio.
        /// </summary>
        /// <param name="id">ID del espacio</param>
        /// <param name="ct">Token de cancelación</param>
        /// <remarks> 
        /// El codigo de union a una residencia dura 1 hora,
        /// en esa hora devolvera el mismo por esa residencia 
        /// </remarks>
        /// <returns>El código de invitación generado</returns>
        [HttpGet("{id}/getCode")]
        public async Task<IActionResult> GetInvitationCode(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            try
            {
                var codigo = await _service.GenerarCodigoInvitacionAsync(id, ct);
                return Ok(new { codigo });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Permite a un usuario unirse a un espacio mediante un código de invitación.
        /// </summary>
        /// <param name="code">Código de invitación del espacio</param>
        /// <param name="dto">Datos del usuario que se une (UsuarioId)</param>
        /// <param name="ct">Token de cancelación</param>
        /// <remarks> Al unir a un usuario se creara un usuarioEspacio de forma automatica y dara error si el usuarioEspacio existe</remarks>
        /// <returns>Confirmación de unión al espacio con información del UsuarioEspacio creado</returns>
        [HttpPost("{code}/usuario")]
        public async Task<IActionResult> JoinSpaceByCode(string code, [FromBody] JoinByCodeDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(code)) return BadRequest("Código requerido");
            if (dto == null || string.IsNullOrWhiteSpace(dto.UsuarioId)) return BadRequest("UsuarioId requerido");

            var result = await _service.UnirUsuarioPorCodigoAsync(code, dto.UsuarioId, ct);
            if (result== null) return NotFound("Código no válido o expirado");

            return Ok(new { message = "Usuario unido al espacio correctamente", usuarioEspacio = result });
        }
    }
}