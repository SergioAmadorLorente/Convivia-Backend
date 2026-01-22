using System;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Datos necesarios para crear una nueva petición de acceso a un espacio.
    /// </summary>
    public class CreatePeticionDto
    {
        /// <summary>
        /// Mensaje o justificación de por qué se solicita acceso al espacio.
        /// </summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// ID del usuario que realiza la petición.
        /// </summary>
        public string IdSolicitante { get; set; } = string.Empty;

        /// <summary>
        /// ID del espacio al que se solicita acceso.
        /// </summary>
        public string IdEspacio { get; set; } = string.Empty;
    }
}
