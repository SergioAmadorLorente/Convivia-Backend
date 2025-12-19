using Convivia.Domain.Entities;
using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Convivia.Domain.Entities
{
    public enum TareaEstado
    {
        Pendiente = 0,
        FueraDePlazo = 1,
        Completada = 2,
        CompletadaFueradePlazo = 3
    }

    public class Tarea
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString("N");

        public DateTime? FechaRealizacion { get; set; }

        public byte[]? Foto { get; set; } // Para almacenar imagen binaria

        // Prorroga ahora representa la duración (TimeSpan) que ha pasado desde el vencimiento
        public TimeSpan? Prorroga { get; set; } // Puede ser null

        // Reemplazamos los booleanos Completada/Disponible por el enum Estado
        public TareaEstado Estado { get; set; } = TareaEstado.Pendiente;

        public string PlantillaId { get; set; } = string.Empty; // obligatorio

        // Single assigned user per task
        public string? UsuarioEspacioId { get; set; }

        public int DiaSemana { get; set; }

        // Fecha limite para tarea puntual o referencia
        public DateTime? FechaLimite { get; set; }

        // Hora límite específica para esta instancia de tarea (si es null usar la de la plantilla)
        public TimeOnly? HoraLimite { get; set; }

        public Tarea()
        {
            // Constructor vacío necesario para la deserialización
        }

        public Tarea(string usuarioEspacioId, string plantillaId)
        {
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new ArgumentException("PlantillaId es obligatoria.", nameof(plantillaId));

            UsuarioEspacioId = usuarioEspacioId;
            Estado = TareaEstado.Pendiente;
            PlantillaId = plantillaId;
        }

    }

}