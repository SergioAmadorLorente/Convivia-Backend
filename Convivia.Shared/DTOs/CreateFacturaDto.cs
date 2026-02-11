using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Datos para crear una nueva factura en un espacio.
    /// </summary>
    public class CreateFacturaDto
    {
        /// <summary>
        /// Nombre o descripción de la factura.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Precio total de la factura en euros.
        /// </summary>
        public decimal Precio { get; set; }

        /// <summary>
        /// Pago mediano calculado por persona o unidad (opcional, se calcula automáticamente si no se proporciona).
        /// </summary>
        public float? PagoMediano { get; set; }

        /// <summary>
        /// Diccionario de deudores donde la clave es el ID del usuario y el valor indica si ha pagado (true) o no (false).
        /// </summary>
        public Dictionary<string, bool> Deudores { get; set; } = new Dictionary<string, bool>();

        /// <summary>
        /// Indica si la factura ha sido pagada completamente.
        /// </summary>
        public bool Pagado { get; set; }

        /// <summary>
        /// ID del usuario (UsuarioId) que crea la factura. Representa quién ha hecho el pago inicial.
        /// </summary>
        public string CreadorFactura { get; set; }
    }
}
