namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// DTO para completar o descompletar una tarea
    /// </summary>
    public class CompletarTareaDto
    {
        /// <summary>
        /// Indica si la tarea debe marcarse como completada (true) o pendiente (false)
        /// </summary>
        public bool TareaCompletada { get; set; }
    }
}
