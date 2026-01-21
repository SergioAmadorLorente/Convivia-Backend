using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Datos necesarios para crear un nuevo espacio compartido.
    /// </summary>
    public class CreateEspacioDto
    {
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
