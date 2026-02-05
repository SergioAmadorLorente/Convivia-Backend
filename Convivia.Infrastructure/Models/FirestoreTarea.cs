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
        public string Estado { get; set; }

        [FirestoreProperty]
        public int DiaSemana { get; set; }

        [FirestoreProperty]
        public string PlantillaId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string? HoraLimite { get; set; }

        [FirestoreProperty]
        public int? UltimaSemanaModificacion { get; set; }
    }
}
