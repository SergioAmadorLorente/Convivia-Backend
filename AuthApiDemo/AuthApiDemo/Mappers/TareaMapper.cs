using AuthApiDemo.DTOs;
using AuthApiDemo.Models;
using System.Collections.Generic;

namespace AuthApiDemo.Mappers
{
    public static class TareaMapper
    {
        // Convierte CreateTareaDto a TareaPersist para guardar en Firestore
        public static TareaPersist ToPersist(CreateTareaDto dto, string idTarea)
        {
            return new TareaPersist
            {
                IdTarea = idTarea,
                Nombre = dto.Nombre,
                fechaLimite = dto.FechaLimite,
                Descripcion = dto.Descripcion,
                UsuarioEspacioIds = dto.UsuarioEspacioIds ?? new List<string>(),
                Karma = dto.Karma,
                Foto = dto.Foto,
                Prorroga = dto.Prorroga,
                Estado = false,
                FechaRealizacion = null
            };
        }

        // Convierte TareaPersist a TareaDto para enviar al cliente
        public static TareaDto ToDto(TareaPersist persist)
        {
            return new TareaDto
            {
                IdTarea = persist.IdTarea,
                Nombre = persist.Nombre,
                FechaLimite = persist.fechaLimite,
                Descripcion = persist.Descripcion,
                UsuarioEspacioIds = persist.UsuarioEspacioIds ?? new List<string>(),
                Karma = persist.Karma,
                Foto = persist.Foto,
                Prorroga = persist.Prorroga,
                Estado = persist.Estado,
                FechaRealizacion = persist.FechaRealizacion
            };
        }
    }
}
