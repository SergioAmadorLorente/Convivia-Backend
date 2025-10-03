using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace AuthApiDemo.Models
{
    [FirestoreData]
    public class TareaPersist
    {
        [FirestoreProperty]
        public string IdTarea { get; set; } = default!;
        [FirestoreProperty]
        public string Nombre { get; set; } = default!;
        [FirestoreProperty]
        public DateTime fechaLimite { get; set; }
        [FirestoreProperty]
        public string? Descripcion { get; set; }
        [FirestoreProperty]
        public List<string> UsuarioEspacioIds { get; set; } = new();
        [FirestoreProperty]
        public int Karma { get; set; } = 10;
        [FirestoreProperty]
        public byte[]? Foto { get; set; }
        [FirestoreProperty]
        public DateTime? Prorroga { get; set; }
        [FirestoreProperty]
        public bool Estado { get; set; } = false;
        [FirestoreProperty]
        public DateTime? FechaRealizacion { get; set; }
        [FirestoreProperty]
        public string? FacturaId { get; set; } // Referencia a la factura asociada
    }
}