using Convivia.Domain.Models;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace Convivia.Infrastructure.Models
{

    [FirestoreData]
    public class FirestoreTarea
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public List<string> UsuarioEspaciosIds { get; set; }

        [FirestoreProperty]
        public DateTime? FechaRealizacion { get; set; }

        [FirestoreProperty]
        public DateTime HoraLimite { get; set; }

        [FirestoreProperty]
        public byte[]? Foto { get; set; } // Para almacenar imagen binaria

        [FirestoreProperty]
        public DateTime? Prorroga { get; set; } // Puede ser null

        [FirestoreProperty]
        public bool Estado { get; set; }

        [FirestoreProperty]
        public string EspacioId { get; set; }

        [FirestoreProperty]
        public string? FacturaId { get; set; }
        [FirestoreProperty]
        public int karma { get; set; } = 0;
    }
}