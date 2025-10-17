
/// TO DO: Implementar el servicio para manejar plantillas de tareas en la aplicación AuthApiDemo.
/// 
/*

using System;
using System.Threading.Tasks;
using AuthApiDemo.Models;
using AuthApiDemo.DTOs;

namespace AuthApiDemo.Services
{
    public class PlantillaTareaService
    {
        public const string COLLECTION = "plantillatareas";
        private readonly IFirebaseService _firebase;

        public PlantillaTareaService(IFirebaseService firebase)
        {
            _firebase = firebase;
        }

        public async Task<PlantillaTareaDto> CreateFromTareaDtoAsync(CreatePlantillaTareaDto dto)
        {
            var plantilla = new PlantillaTarea
            {
                Nombre = dto.Nombre,
                FechaCreacion = DateTime.UtcNow,
                PuntosKarma = dto.PuntosKarma,
                Disponible = true,
                DiasRepeticion = dto.DiasRepeticion ?? new List<DayOfWeek>()
            };

            var id = await _firebase.AddAsync(COLLECTION, plantilla);

            return new PlantillaTareaDto
            {
                PlantillaId = id,
                Nombre = plantilla.Nombre,
                FechaCreacion = plantilla.FechaCreacion,
                PuntosKarma = plantilla.PuntosKarma,
                Disponible = plantilla.Disponible,
                DiasRepeticion = plantilla.DiasRepeticion
            };
        }
    }
}
*/