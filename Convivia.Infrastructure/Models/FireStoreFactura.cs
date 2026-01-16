using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Convivia.Infrastructure.Models
{
    [FirestoreData]
    public class FireStoreFactura
    {
        [FirestoreProperty("Id")]
        public string Id { get; set; }

        [FirestoreProperty("Nombre")]
        public string Nombre { get; set; }

        [FirestoreProperty("Precio")]
        public float Precio { get; set; }

        [FirestoreProperty("PagoMediano")]
        public float PagoMediano { get; set; }

        [FirestoreProperty("Deudores")]
        public Dictionary<string, bool> Deudores { get; set; } = new Dictionary<string, bool>();

        [FirestoreProperty("Pagado")]
        public bool Pagado { get; set; }

        [FirestoreProperty("DocumentoUrl")]
        public string? DocumentoUrl { get; set; }

        [FirestoreProperty("FechaCreacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public FireStoreFactura()
        {
            // Asegurar explícitamente Kind = Utc en caso de que algún mapper deje un DateTime sin especificar
            FechaCreacion = DateTime.SpecifyKind(FechaCreacion, DateTimeKind.Utc);
        }
        
    }
}