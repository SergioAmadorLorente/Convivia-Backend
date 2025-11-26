/*
using Convivia.Application.DTOs;
using Convivia.Domain.Models;
using System.Collections.Generic;

namespace Convivia.Application.Mappers
{
    public static class PlantillaTareaMapper
    {
        // Convierte CreatePlantillaTareaDto a PlantillaTareaPersist para guardar en Firestore
        public static PlantillaTareaPersist ToPersist(CreatePlantillaTareaDto dto, string PlantillaId)
        {
            return new PlantillaTareaPersist
            {
                PlantillaId = PlantillaId,
                Nombre = dto.Nombre,
                FechaCreacion = dto.FechaCreacion,
                PuntosKarma = dto.PuntosKarma,
                Disponible = dto.Disponible,
                DiasRepeticion = dto.DiasRepeticion ?? new List<DayOfWeek>()
            };
        }

        // Convierte PlantillaTareaPersist a PlantillaTareaDto para enviar al cliente
        public static PlantillaTareaDto ToDto(PlantillaTareaPersist persist)
        {
            return new PlantillaTareaDto
            {
                PlantillaId = persist.PlantillaId,
                Nombre = persist.Nombre,
                FechaCreacion = persist.FechaCreacion,
                PuntosKarma = persist.PuntosKarma,
                Disponible = persist.Disponible,
                DiasRepeticion = persist.DiasRepeticion ?? new List<DayOfWeek>()
            };
        }
    }
}
*/