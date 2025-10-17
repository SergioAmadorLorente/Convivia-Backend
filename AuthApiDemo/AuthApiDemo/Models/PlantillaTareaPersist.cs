using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace AuthApiDemo.Models
{

    [FirestoreData]
    public class PlantillaTareaPersist
    {
        [FirestoreProperty]
        public string PlantillaId { get; set; } = default!;
        [FirestoreProperty]
        public string Nombre { get; set; } = default!;
        [FirestoreProperty]
        public DateTime FechaCreacion { get; set; }
        [FirestoreProperty]
        public int PuntosKarma { get; set; } = 10;
        [FirestoreProperty]
        public bool Disponible { get; set; } = true;
        [FirestoreProperty]
        public List<DayOfWeek> DiasRepeticion { get; set; } = new();
    }
}