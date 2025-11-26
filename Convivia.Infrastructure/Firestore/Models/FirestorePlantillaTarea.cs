using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace Convivia.Infrastructure.Models
{

    [FirestoreData]
    public class FirestorePlantillaTarea
    {
        [FirestoreProperty]
        public string PlantillaId { get; set; } = new Guid().ToString();
        [FirestoreProperty]
        public string Nombre { get; set; } = default!;
        [FirestoreProperty]
        public DateTime FechaCreacion { get; set; }
        [FirestoreProperty]
        public int PuntosKarma { get; set; } = 10;
        [FirestoreProperty]
        public bool Disponible { get; set; } = true;
        [FirestoreProperty]
        public List<int> DiasRepeticion { get; set; } = new();
        [FirestoreProperty]
        public List<string> TareasId { get; set; } = new();
    }
}