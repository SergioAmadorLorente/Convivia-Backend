using Convivia.Domain.Entities;
using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Convivia.Domain.Entities
{

    public class Tarea
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString("N");

        public DateTime? FechaRealizacion { get; set; }

        public byte[]? Foto { get; set; } // Para almacenar imagen binaria

        public DateTime? Prorroga { get; set; } // Puede ser null

        public bool Disponible { get; set; }
        public bool Completada { get; set; }

        public string PlantillaId { get; set; } = string.Empty; // obligatorio

        // Single assigned user per task
        public string? UsuarioEspacioId { get; set; }

        public int DiaSemana { get; set; }

        // Fecha limite para tarea puntual o referencia
        public DateTime? FechaLimite { get; set; }

        public Tarea()
        {
            // Constructor vacío necesario para la deserialización
        }

        public Tarea(string usuarioEspacioId, string plantillaId)
        {
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new ArgumentException("PlantillaId es obligatoria.");

            UsuarioEspacioId = usuarioEspacioId;
            Disponible = false; // Por defecto, la tarea está incompleta
            PlantillaId = plantillaId;
        }

    }

}