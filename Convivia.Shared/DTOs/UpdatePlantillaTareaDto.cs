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
        /// Fecha límite para tareas puntuales o límite de repetición para tareas repetidas.
        /// Formato: YYYY-MM-DD
        /// </summary>
        public DateOnly? FechaLimite { get; set; }

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
        /// Asignación lineal posición a posición: usuario[i] → día[i].
        /// Si hay menos usuarios que días nuevos: días sin usuario asignado quedan en null.
        /// NO puede haber más usuarios que días nuevos.
        /// Ejemplo: 4 días + 2 usuarios → día[0]=usuario[0], día[1]=usuario[1], día[2]=null, día[3]=null
        /// </summary>
        public List<string>? UsuariosAsignacion { get; set; }
    }
}