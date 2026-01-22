using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;


namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Representa un espacio compartido con todos sus datos.
    /// </summary>
    public class EspacioDto
    {
        public EspacioDto() { }

        /// <summary>
        /// Identificador único del espacio.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Nombre del espacio compartido.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Dirección física del espacio (opcional).
        /// </summary>
        public string? Direccion { get; set; }
    }
}
