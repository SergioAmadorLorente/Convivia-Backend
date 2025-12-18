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
        public string? Id { get; set; }

        [FirestoreProperty]
        public string? UsuarioEspacioId { get; set; }

        [FirestoreProperty]
        public DateTime? FechaRealizacion { get; set; }

        [FirestoreProperty]
        public byte[]? Foto { get; set; } // Para almacenar imagen binaria

        [FirestoreProperty]
        public DateTime? Prorroga { get; set; } // Puede ser null

        [FirestoreProperty]
        public bool Disponible { get; set; }
        [FirestoreProperty]
        public bool Completada { get; set; }

        [FirestoreProperty]
        public int DiaSemana { get; set; }

        [FirestoreProperty]
        public string PlantillaId { get; set; } = string.Empty;

        [FirestoreProperty]
        public DateTime? FechaLimite { get; set; }
    }
}