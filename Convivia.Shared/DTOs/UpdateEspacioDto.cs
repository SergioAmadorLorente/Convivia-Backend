namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Datos para actualizar un espacio existente. Todos los campos son opcionales.
    /// </summary>
    public class UpdateEspacioDto
    {
        /// <summary>
        /// Nuevo nombre del espacio (opcional).
        /// </summary>
        public string? Nombre { get; set; }

        /// <summary>
        /// Nueva dirección del espacio (opcional).
        /// </summary>
        public string? Direccion { get; set; }
    }
}
