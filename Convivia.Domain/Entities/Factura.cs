using Convivia.Domain.Entities;
using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

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
        
        public string Id_Factura { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Nombre o concepto de la factura.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Importe total de la factura.
        /// </summary>
        public float Precio { get; set; }

        /// <summary>
        /// Mapa de reparto: cada usuario y la cantidad que debe pagar. Solo lectura desde fuera.
        /// </summary>        
        private Dictionary<UsuarioEspacio, float> _repartoMap = new Dictionary<UsuarioEspacio, float>();
        public IReadOnlyDictionary<UsuarioEspacio, float> RepartoMap => _repartoMap;

        /// <summary>
        /// Indica si la factura está pagada completamente.
        /// </summary>
        public bool Pagado { get; set; }

        /// <summary>
        /// Documento o imagen asociada a la factura (opcional).
        /// </summary>
        public byte[]? Documento { get; set; }

        /// <summary>
        /// Tarea asociada a la factura (opcional).
        /// </summary>
        public Tarea? Tarea { get; set; }

        /// <summary>
        /// Constructor por defecto para deserialización o pruebas.
        /// </summary>
        public Factura() { }
    }
}