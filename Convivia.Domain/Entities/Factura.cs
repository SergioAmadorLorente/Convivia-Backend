using System;
using System.Collections.Generic;

namespace Convivia.Domain.Entities
{
    /// <summary>
    /// Representa una factura con reparto de pago entre usuarios, estado de pago y posible documento/tarea asociada.
    /// </summary>
    public class Factura
    {
        /// <summary>
        /// Identificador único de la factura.
        /// </summary>
        
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Nombre o concepto de la factura.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Importe total de la factura.
        /// </summary>
        public float Precio { get; set; }

        /// <summary>
        /// Importe de reparto por persona (cuando se aplica reparto equitativo).
        /// </summary>
        public float PagoMediano { get; set; }

        /// <summary>
        /// Reparto representado por usuario (UsuarioEspacioId) -> pagado (true/false).
        /// </summary>
        public Dictionary<string, bool> Deudores { get; set; } = new Dictionary<string, bool>();

        /// <summary>
        /// Indica si la factura está pagada completamente.
        /// </summary>
        public bool Pagado { get; set; }

        /// <summary>
        /// Documento o imagen asociada a la factura (opcional).
        /// </summary>
        public string? DocumentoUrl { get; set; }

        /// <summary>
        /// Fecha creacion de la factura
        /// </summary>

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Constructor por defecto para deserialización o pruebas.
        /// </summary>
        public Factura() { }
    }
}