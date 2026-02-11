namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Representa las estadísticas de tareas de un usuario en un espacio
    /// </summary>
    public class TareaEstadisticasDto
    {
        /// <summary>
        /// Número de tareas completadas
        /// </summary>
        public int Completadas { get; set; }

        /// <summary>
        /// Número de tareas pendientes (no completadas)
        /// </summary>
        public int Pendientes { get; set; }

        /// <summary>
        /// Número de tareas pendientes que están retrasadas (fecha de vencimiento ya pasó)
        /// </summary>
        public int Tardes { get; set; }
    }
}
