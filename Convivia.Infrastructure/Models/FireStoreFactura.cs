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
        public string IdFactura { get; set; }

        [FirestoreProperty("Nombre")]
        public string Nombre { get; set; }

        [FirestoreProperty("Precio")]
        public float Precio { get; set; }

        [FirestoreProperty("Reparto")]
        public Dictionary<string, float> Reparto { get; set; } = new Dictionary<string, float>();

        [FirestoreProperty("RepartoKeys")]
        public List<string> RepartoKeys { get; set; } = new();

        [FirestoreProperty("Pagado")]
        public bool Pagado { get; set; }

        [FirestoreProperty("DocumentoUrl")]
        public byte[]? DocumentoUrl { get; set; }

        [FirestoreProperty("TareaId")]
        public string? TareaId { get; set; }

        [FirestoreProperty("FechaCreacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public FireStoreFactura()
        {
            // Asegurar explícitamente Kind = Utc en caso de que algún mapper deje un DateTime sin especificar
            FechaCreacion = DateTime.SpecifyKind(FechaCreacion, DateTimeKind.Utc);
        }
        public void SyncRepartoKeys()
        {
            RepartoKeys = Reparto.Keys.ToList(); //metodo auxiliar para sincronizar reparto y repartokeys
        }
    }
}