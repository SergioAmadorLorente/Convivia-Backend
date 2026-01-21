using Convivia.Application.Services;
using Convivia.Infrastructure.Services;
using Convivia.Shared.DTOs;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convivia.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de plantillas de tareas dentro de espacios.
    /// Las plantillas definen tareas recurrentes que se asignan automáticamente.
    /// </summary>
    [ApiController]
    [Route("api/espacio/{espacioid}")]
    [Produces("application/json")]
    public class PlantillaTareaController : ControllerBase
    {
        private readonly PlantillaTareaService _service;

        public PlantillaTareaController(PlantillaTareaService service)
        {
            _service = service;
        }

        /// <summary>
        /// Actualiza parcialmente una plantilla de tarea.
        /// </summary>
        /// <param name="espacioid">ID del espacio</param>
        /// <param name="id">ID de la plantilla de tarea</param>
        /// <param name="dto">Campos a actualizar (los no enviados no se modifican)</param>
        [HttpPatch("{id}")]
        public async Task<ActionResult<PlantillaTareaDto>> UpdatePlantilla(string espacioid, string id, UpdatePlantillaTareaDto dto)
        {
            var plantillaDto = await _service.UpdateAsync(espacioid, id, dto);
            return plantillaDto != null ? Ok(plantillaDto) : NotFound();
        }
    }
}