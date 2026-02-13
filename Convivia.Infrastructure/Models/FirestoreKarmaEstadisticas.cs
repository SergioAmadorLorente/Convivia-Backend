using Google.Cloud.Firestore;
using System;

namespace Convivia.Infrastructure.Models
{
    [FirestoreData]
    public class FirestoreKarmaEstadisticas
    {
        [FirestoreProperty("Id")]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("UsuarioEspacioId")]
        public string UsuarioEspacioId { get; set; } = string.Empty;

        [FirestoreProperty("KarmaTotal")]
        public int KarmaTotal { get; set; }

        [FirestoreProperty("KarmaSemanal")]
        public int KarmaSemanal { get; set; }

        [FirestoreProperty("KarmaMensual")]
        public int KarmaMensual { get; set; }

        [FirestoreProperty("UltimaSemana")]
        public int? UltimaSemana { get; set; }

        [FirestoreProperty("UltimoMes")]
        public int? UltimoMes { get; set; }

        [FirestoreProperty("UltimaActualizacion")]
        public DateTime UltimaActualizacion { get; set; }
    }
}
