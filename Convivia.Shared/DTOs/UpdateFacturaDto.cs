using Convivia.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Datos para actualizar una factura existente. Todos los campos son opcionales.
    /// </summary>
    public class UpdateFacturaDto
    {
        /// <summary>
        /// Nuevo nombre o descripción de la factura (opcional).
        /// </summary>
        public string? Nombre { get; set; }

        /// <summary>
        /// Nuevo precio total de la factura en euros (opcional).
        /// </summary>
        public decimal? Precio { get; set; }

        /// <summary>
        /// Nuevo pago mediano calculado por persona o unidad (opcional).
        /// </summary>
        public float? PagoMediano { get; set; }

        /// <summary>
        /// Nuevo diccionario de deudores (opcional). La clave es el ID del usuario y el valor indica si ha pagado.
        /// </summary>
        public Dictionary<string, bool>? Deudores { get; set; } = new Dictionary<string, bool>();

        /// <summary>
        /// Nuevo estado de pago de la factura (opcional).
        /// </summary>
        public bool? Pagado { get; set; }

        /// <summary>
        /// Nuevo creador de la factura (opcional). ID del usuario (UsuarioEspacioId).
        /// </summary>
        public string? CreadorFactura { get; set; }
    }
}