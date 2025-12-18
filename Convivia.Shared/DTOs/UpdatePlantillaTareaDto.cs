using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Convivia.Shared.DTOs
{
    public class UpdatePlantillaTareaDto
    {
        public string? Nombre { get; set; }

        public TimeOnly? HoraLimite { get; set; }

        public string? Descripcion { get; set; }

        [Range(5, 50, ErrorMessage = "Karma debe ser 5, 15, 25 o 50")]
        public int? karma { get; set; }

        public string? FacturaId { get; set; } // Referencia a la factura asociada

        public List<string>? TareasId { get; set; }

        public int? GracePeriodMinutes { get; set; }

        public DateTime? FechaFin { get; set; }
    }
}