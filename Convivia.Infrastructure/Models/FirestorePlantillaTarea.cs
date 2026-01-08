using Google.Cloud.Firestore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Collections.Generic;

namespace Convivia.Infrastructure.Models
{

    [FirestoreData]
    public class FirestorePlantillaTarea
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Nombre { get; set; } = default!;

        [FirestoreProperty]
        public string? Descripcion { get; set; }

        [FirestoreProperty]
        public DateTime FechaCreacion { get; set; }
        [FirestoreProperty]
        public int karma { get; set; }
        [FirestoreProperty]
        public List<int> DiasRepeticion { get; set; } = new();
        [FirestoreProperty]
        public List<string> TareasId { get; set; } = new();
        [FirestoreProperty]
        public string EspacioId { get; set; }
        [FirestoreProperty]
        public int IntervalWeeks { get; set; }
        [FirestoreProperty]
        public string TimeZoneId { get; set; } = "Europe/Madrid";
        [FirestoreProperty]
        public string? StartDate { get; set; }
        [FirestoreProperty]
        public string? FacturaId { get; set; }

    }
}