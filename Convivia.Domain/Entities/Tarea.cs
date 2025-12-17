using Convivia.Domain.Entities;
using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Convivia.Domain.Entities
{

    public class Tarea
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString("N");

        

        public List<UsuarioEspacio> Usuarios { get; set; } = new();
        
        public DateTime? FechaRealizacion { get; set; }

        public byte[]? Foto { get; set; } // Para almacenar imagen binaria

        public DateTime? Prorroga { get; set; } // Puede ser null

        public bool Disponible { get; set; }
        public bool Completada { get; set; }

        public string PlantillaId { get; set; } = string.Empty; // obligatorio
        public List<string> UsuarioEspaciosIds { get; set; } = new();

        public int DiaSemana { get; set; }

        // Optional sala id where task applies
        public string? SalaId { get; set; }

        public Tarea()
        {
            // Constructor vacío necesario para la deserialización
        }

        public Tarea(List<string> usuarioEspaciosIds, string plantillaId)
        {
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new ArgumentException("PlantillaId es obligatoria.");

            UsuarioEspaciosIds = usuarioEspaciosIds;
            Disponible = false; // Por defecto, la tarea está incompleta
            PlantillaId = plantillaId;
        }

    }

}