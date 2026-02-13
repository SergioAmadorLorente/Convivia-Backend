using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Representa una factura completa con todos sus datos y metadatos.
    /// </summary>
    public class FacturaDto
    {
        public FacturaDto() { }

        /// <summary>
        /// Identificador único de la factura.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Nombre o descripción de la factura.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Precio total de la factura en euros.
        /// </summary>
        public float Precio { get; set; }

        /// <summary>
        /// Pago mediano calculado por persona o unidad.
        /// </summary>
        public float PagoMediano { get; set; }

        /// <summary>
        /// Diccionario de deudores donde la clave es el ID del usuario y el valor indica si ha pagado (true) o no (false).
        /// </summary>
        public Dictionary<string, bool> Deudores { get; set; } = new Dictionary<string, bool>();

        /// <summary>
        /// Indica si la factura ha sido pagada completamente por todos los deudores.
        /// </summary>
        public bool Pagado { get; set; }

        /// <summary>
        /// ID del usuario (UsuarioId) que creó la factura.
        /// </summary>
        public string CreadorFactura { get; set; }

        /// <summary>
        /// Indica si la factura tiene una imagen asociada (documento escaneado, foto, etc.).
        /// Use el endpoint GET /factura/{id}/imagen para obtener la imagen.
        /// </summary>
        public bool TieneImagen { get; set; }

        /// <summary>
        /// Fecha y hora de creación de la factura en formato UTC.
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
