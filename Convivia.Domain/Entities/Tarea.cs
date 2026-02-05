using Convivia.Domain.Entities;
using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Convivia.Domain.Entities
{
    public enum TareaEstado
    {
        Pendiente = 0,
        Completada = 1
    }

    public class Tarea
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString("N");

        public DateTime? FechaRealizacion { get; set; }

        public TareaEstado Estado { get; set; } = TareaEstado.Pendiente;

        public string PlantillaId { get; set; } = string.Empty;

        public string? UsuarioEspacioId { get; set; }

        public int DiaSemana { get; set; }

        public TimeOnly? HoraLimite { get; set; }

        public Tarea()
        {
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
/// <INFO>
///    PlantillaTareea > Tarea * Repeticions semanals
///    Cuando pasa la semana las tareas vuelven a estar pendientes
///    
/// </INFO>