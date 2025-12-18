using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Convivia.Shared.DTOs
{
    public class CreatePlantillaTareaDto
    {
        [Required]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(5, 50, ErrorMessage = "Karma debe ser 5, 15, 25 o 50")]
        public int karma { get; set; }

        /// <summary>
        /// Días de repetición semanal (0=Domingo, 6=Sábado).
        /// Si está vacío = tarea puntual.
        /// </summary>
        public List<int> DiasRepeticion { get; set; } = new();

        /// <summary>
        /// Hora límite para completar la tarea.
        /// </summary>
        [Required]
        public TimeOnly HoraLimite { get; set; }

        /// <summary>
        /// Fecha límite (para puntual) o referencia para repetida.
        /// </summary>
        public DateTime? FechaLimite { get; set; }

        /// <summary>
        /// Oculto al cliente: rellenado por TareaService con IDs de tareas creadas.
        /// </summary>
        [JsonIgnore]
        public List<string> TareasId { get; set; } = new();

        /// <summary>
        /// TimeZoneId: rellenado por defecto en PlantillaTareaService.
        /// </summary>
        [JsonIgnore]
        public string? TimeZoneId { get; set; }

        /// <summary>
        /// ID de factura asociada (opcional).
        /// </summary>
        public string? FacturaId { get; set; }

        /// <summary>
        /// Período de gracia en minutos antes de marcar overdue (opcional).
        /// </summary>
        public int? GracePeriodMinutes { get; set; }

        /// <summary>
        /// ¿Asignar usuarios aleatoriamente a las tareas de esta plantilla?
        /// Solo aplicable si DiasRepeticion.Count > 0 (tarea repetida).
        /// </summary>
        public bool AsignarUsuariosAleatorio { get; set; } = false;

        /// <summary>
        /// Lista de usuarios a asignar a las tareas (en orden rotativo o aleatorio).
        /// </summary>
        public List<string> UsuariosAsignacion { get; set; } = new();
    }
}