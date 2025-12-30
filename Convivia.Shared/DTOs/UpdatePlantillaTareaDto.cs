using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Convivia.Shared.DTOs
{
    public class UpdatePlantillaTareaDto
    {
        public string? Nombre { get; set; }

        /// <summary>
        /// Hora límite opcional, solo se usa para crear nuevas instancias de tarea cuando se añaden días de repetición.
        /// Formato: HH:mm (se almacena como HH:mm en la BD).
        /// </summary>
        public TimeOnly? HoraLimite { get; set; }

        public string? Descripcion { get; set; }

        /// <summary>
        /// Karma asociado a la plantilla.
        /// Valores válidos: 5, 15, 25, 50.
        /// </summary>
        [Range(5, 50, ErrorMessage = "Karma debe ser 5, 15, 25 o 50")]
        public int? karma { get; set; }

        /// <summary>
        /// Período de gracia en minutos antes de marcar overdue.
        /// Rango: 1-60 minutos (máximo 1 hora).
        /// </summary>
        public int? GracePeriodMinutes { get; set; }

        /// <summary>
        /// Fecha de finalización de la plantilla (solo para plantillas repetidas).
        /// Formato: YYYY-MM-DD (e.g., "2025-12-30")
        /// No se puede modificar StartDate (se establece como hoy cuando se crea).
        /// </summary>
        public DateOnly? FechaFin { get; set; }

        /// <summary>
        /// Días de repetición semanal a actualizar.
        /// Formato del cliente: 0=Lunes, 1=Martes, 2=Miércoles, 3=Jueves, 4=Viernes, 5=Sábado, 6=Domingo
        /// Si se proporciona y es diferente a los días actuales:
        /// - Se crearán nuevas tareas para los días añadidos
        /// - Se eliminarán tareas para los días removidos
        /// Null/vacío = no cambiar días de repetición.
        /// Valores permitidos: 0-6 (sin duplicados, únicos).
        /// </summary>
        public List<int>? DiasRepeticion { get; set; }

        /// <summary>
        /// Usuarios a asignar a las tareas cuando se añaden nuevos días de repetición.
        /// Requerido si DiasRepeticion contiene días nuevos.
        /// Si se proporciona 1 usuario: se asigna a todos los días nuevos.
        /// Si se proporcionan múltiples: el número debe coincidir con los días nuevos.
        /// </summary>
        public List<string>? UsuariosAsignacion { get; set; }
    }
}