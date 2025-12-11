using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Convivia.Shared.DTOs
{
    public class UpdatePlantillaTareaDto
    {
        public string? Nombre { get; set; }

        public DateTime? FechaCreacion { get; set; }

        public int? karma { get; set; }

        public List<int>? DiasRepeticion { get; set; }

        public List<string>? TareasId { get; set; } = new List<string>();

        public TimeOnly? HoraLimite { get; set; }

    }
}