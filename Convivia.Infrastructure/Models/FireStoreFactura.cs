using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Convivia.Infrastructure.Models
{
    [FirestoreData]
    public class Factura
    {
        [FirestoreProperty("Id")]
        public string IdFactura { get; set; }

        [FirestoreProperty("Nombre")]
        public string Nombre { get; set; }

        [FirestoreProperty("Precio")]
        public float Precio { get; set; }

        // Reparto serializable: clave = UsuarioId, valor = importe a pagar (recordar per al mapper)
        [FirestoreProperty("Reparto")]
        public Dictionary<string, decimal> Reparto { get; private set; } = new Dictionary<string, decimal>();

        [FirestoreProperty("Pagado")]
        public bool Pagado { get; private set; }

        [FirestoreProperty("DocumentoUrl")]
        public byte[]? DocumentoUrl { get; private set; }

        [FirestoreProperty("TareaId")]
        public string? TareaId { get; private set; }

        [FirestoreProperty("FechaCreacion")]
        public DateTime FechaCreacion { get; private set; }

        public Factura() {}
    }
}