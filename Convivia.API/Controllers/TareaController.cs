using Convivia.Application.Services;
using Convivia.Infrastructure.Services;
using Convivia.Shared.DTOs;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace Convivia.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de tareas dentro de espacios.
    /// Permite crear, consultar, actualizar, eliminar y filtrar tareas asignadas a usuarios.
    /// </summary>
    [ApiController]
    [Route("api/espacios/{espacioid}/tareas")]
    [Produces("application/json")]
    public class TareaController : ControllerBase
    {
        private readonly TareaService _service;
        private readonly PlantillaTareaService _ptservice;

        public TareaController(TareaService service, PlantillaTareaService ptservice)
        {
            _service = service;
            _ptservice = ptservice;
        }

        /// <summary>
        /// Crea una nueva tarea en el espacio especificado.
        /// </summary>
        /// <param name="espacioid">ID del espacio</param>
        /// <param name="dto">Datos de la tarea a crear</param>
        [HttpPost]
        public async Task<ActionResult<List<string>>> CreateTarea(string espacioid, [FromBody] CreateTareaDto dto)
        {
            var tareaIds = await _service.AddAsync(espacioid, dto);
            return CreatedAtAction(nameof(GetTareasByEspacioId), new { espacioid = espacioid }, tareaIds);
        }

        /// <summary>
        /// Obtiene una tarea específica por su ID.
        /// </summary>
        /// <param name="espacioid">ID del espacio</param>
        /// <param name="plantillaId">ID de la plantilla de tarea</param>
        /// <param name="tareaId">ID de la tarea</param>
        [HttpGet("{plantillaId}/{tareaId}")]
        public async Task<ActionResult<TareaDto>> GetTareaById(string espacioid, string plantillaId, string tareaId)
        {
            var tareaDto = await _service.GetByEspacioAndPlantillaAndTareaAsync(espacioid, plantillaId, tareaId);
            return tareaDto != null ? Ok(tareaDto) : NotFound();
        }

        /// <summary>
        /// Obtiene todas las plantillas de tareas de un espacio.
        /// </summary>
        /// <param name="espacioid">ID del espacio</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlantillaTareaDto>>> GetTareasByEspacioId(string espacioid)
        {
            var list = await _ptservice.GetAllByEspacioAsync(espacioid);
            return !list.Any() ? NotFound() : Ok(list);
        }

        /// <summary>
        /// Filtra tareas por día de la semana, estado y/o usuario asignado.
        /// </summary>
        /// <param name="espacioid">ID del espacio</param>
        /// <param name="diaSemana">Día de la semana (0-6, opcional)</param>
        /// <param name="estado">Estado de la tarea (opcional)</param>
        /// <param name="usuarioId">ID del usuario asignado (opcional)</param>
        /// <returns>Lista de tareas que cumplen los filtros especificados</returns>
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<TareaDto>>> Filter(
            string espacioid, 
            [FromQuery] int? diaSemana, 
            [FromQuery] string? estado,
            [FromQuery] string? usuarioId)
        {
            try
            {
                var res = await _service.FilterAsync(espacioid, diaSemana, estado, usuarioId);
                return (res == null || !res.Any()) ? NotFound() : Ok(res);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Elimina una tarea.
        /// </summary>
        /// <param name="espacioid">ID del espacio</param>
        /// <param name="id">ID de la tarea a eliminar</param>
        /// <returns>Sin contenido si se eliminó correctamente</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTarea(string espacioid, string id)
        {
            var deleted = await _service.DeleteAsync(espacioid, id);
            return deleted ? NoContent() : NotFound();
        }

        /// <summary>
        /// Actualiza completamente una tarea (overwrite). Reemplaza todos los campos.
        /// </summary>
        /// <param name="espacioid">ID del espacio</param>
        /// <param name="plantillaId">ID de la plantilla de tarea</param>
        /// <param name="tareaId">ID de la tarea a actualizar</param>
        /// <param name="dto">Nuevos datos completos de la tarea</param>
        /// <param name="ct">Token de cancelación</param>
        [HttpPut("{plantillaId}/{tareaId}")]
        public async Task<ActionResult<TareaDto>> PutOverwrite(string espacioid, string plantillaId, string tareaId, [FromBody] UpdateTareaDto dto, CancellationToken ct)
        {
            var updated = await _service.UpdateCompleteAsync(espacioid, plantillaId, tareaId, dto, ct);
            return updated != null ? Ok(updated) : NotFound();
        }

        /// <summary>
        /// Actualiza una tarea fusionando los campos enviados con los existentes (merge).
        /// </summary>
        /// <param name="espacioid">ID del espacio</param>
        /// <param name="plantillaId">ID de la plantilla de tarea</param>
        /// <param name="tareaId">ID de la tarea a actualizar</param>
        /// <param name="dto">Campos a actualizar (solo los enviados se modifican)</param>
        /// <param name="ct">Token de cancelación</param>
        [HttpPut("{plantillaId}/{tareaId}/merge")]
        public async Task<ActionResult<TareaDto>> PutMerge(string espacioid, string plantillaId, string tareaId, [FromBody] UpdateTareaDto dto, CancellationToken ct)
        {
            var updated = await _service.UpdateMergeAsync(espacioid, plantillaId, tareaId, dto, ct);
            return updated != null ? Ok(updated) : NotFound();
        }

        /// <summary>
        /// Actualiza parcialmente una tarea. Solo actualiza los campos enviados.
        /// </summary>
        /// <param name="espacioid">ID del espacio</param>
        /// <param name="plantillaId">ID de la plantilla de tarea</param>
        /// <param name="tareaId">ID de la tarea a actualizar</param>
        /// <param name="dto">Campos a actualizar (los no enviados no se modifican)</param>
        /// <param name="ct">Token de cancelación</param>
        [HttpPatch("{plantillaId}/{tareaId}")]
        public async Task<ActionResult<TareaDto>> Patch(string espacioid, string plantillaId, string tareaId, [FromBody] UpdateTareaDto dto, CancellationToken ct)
        {
            var updated = await _service.UpdatePartialAsync(espacioid, plantillaId, tareaId, dto, ct);
            return updated != null ? Ok(updated) : NotFound();
        }
    }
}