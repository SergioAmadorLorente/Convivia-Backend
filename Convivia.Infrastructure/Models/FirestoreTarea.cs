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

        // Guardamos la prorroga como número de segundos (TimeSpan) en Firestore mediante DateTime como convencion: usar TimeSpan ticks no es soportado por Firestore directamente.
        // Para compatibilidad simple, seguiremos guardando DateTime? representando 'DateTime.UtcNow - TimeSpan' no es ideal, mejor almacenar segundos en un campo "ProrrogaSegundos".
        // Pero para mínimo cambio, almacenamos Prorroga como double segundos

        [FirestoreProperty]
        public double? ProrrogaSegundos { get; set; } // segundos de prorroga si existe

        // Guardamos el enum como int en Firestore
        [FirestoreProperty]
        public string Estado { get; set; } // TareaEstado.Pendiente

        [FirestoreProperty]
        public int DiaSemana { get; set; }

        [FirestoreProperty]
        public string PlantillaId { get; set; } = string.Empty;

        [FirestoreProperty]
        public DateTime? FechaLimite { get; set; }

        // Store HoraLimite as string "HH:mm" because Firestore doesn't have TimeOnly natively
        [FirestoreProperty]
        public string? HoraLimite { get; set; }
    }
}