using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Application.Services;

namespace Convivia.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de usuarios del sistema.
    /// Permite crear, consultar, actualizar y eliminar usuarios, así como buscar por correo electrónico.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _service;

        public UsuarioController(UsuarioService service)
        {
            _service = service;
        }

        /// <summary>
        /// Crea un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="model">Datos del usuario a crear (nombre, email, password)</param>
        /// <param name="ct">Token de cancelación</param>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUsuarioDto model, CancellationToken ct)
        {
            if (model == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(model.Nombre)) return BadRequest("Nombre es requerido.");
            if (string.IsNullOrWhiteSpace(model.Email)) return BadRequest("Email es requerido.");
            if (string.IsNullOrWhiteSpace(model.Password)) return BadRequest("Password es requerido.");


            var created = await _service.CrearUsuarioAsync(model, ct);
            return Ok(created);
        }

        /// <summary>
        /// Obtiene un usuario específico por su ID.
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="ct">Token de cancelación</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var usuario = await _service.ObtenerUsuarioAsync(id, ct);
            if (usuario == null) return NotFound();
            return Ok(usuario);
        }
        /// <summary>
        /// Obtiene todos los usuarios del sistema.
        /// </summary>
        /// <param name="ct">Token de cancelación</param>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _service.ListarTodasAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Obtiene un usuario por su correo electrónico.
        /// </summary>
        /// <param name="correo">Correo electrónico del usuario</param>
        /// <param name="ct">Token de cancelación</param>
        [HttpGet("correo/{correo}")]
        public async Task<IActionResult> GetByEmail(string correo, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(correo)) return BadRequest("El correo es requerido");

            var usuario = await _service.ObtenerPorEmailAsync(correo, ct);
            if (usuario == null) return NotFound($"No se encontró usuario con el correo: {correo}");
            return Ok(usuario);
        }
        /// <summary>
        /// Actualiza completamente un usuario (overwrite). Reemplaza todos los campos.
        /// </summary>
        /// <param name="id">ID del usuario a actualizar</param>
        /// <param name="model">Nuevos datos completos del usuario</param>
        /// <param name="ct">Token de cancelación</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOverwrite(string id, [FromBody] UpdateUsuarioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarUsuarioCompletoAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Actualiza un usuario fusionando los campos enviados con los existentes (merge).
        /// </summary>
        /// <param name="id">ID del usuario a actualizar</param>
        /// <param name="model">Campos a actualizar (solo los enviados se modifican)</param>
        /// <param name="ct">Token de cancelación</param>
        [HttpPut("{id}/merge")]
        public async Task<IActionResult> PutMerge(string id, [FromBody] UpdateUsuarioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarUsuarioMergeAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Actualiza parcialmente un usuario. Solo actualiza los campos enviados.
        /// </summary>
        /// <param name="id">ID del usuario a actualizar</param>
        /// <param name="model">Campos a actualizar (los no enviados no se modifican)</param>
        /// <param name="ct">Token de cancelación</param>
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(string id, [FromBody] UpdateUsuarioDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null) return BadRequest();

            var updated = await _service.ActualizarUsuarioParcialAsync(id, model, ct);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        /// <summary>
        /// Elimina un usuario del sistema.
        /// </summary>
        /// <param name="id">ID del usuario a eliminar</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Sin contenido si se eliminó correctamente</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var resultat = await _service.EliminarUsuarioAsync(id, ct);
            return resultat ? NoContent() : NotFound();
        }

    }
}