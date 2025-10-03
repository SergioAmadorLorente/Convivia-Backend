using AuthApiDemo.DTOs;
using AuthApiDemo.Models;
using System.Collections.Generic;

namespace AuthApiDemo.Mappers
{
    public static class EspacioMapper
    {
        public static EspacioPersist ToPersist(CreateEspacioDto dto, string idEspacio)
        {
            return new EspacioPersist
            {
                Id_Espacio = idEspacio,
                Nombre = dto.Nombre,
                Direccion = dto.Direccion,
                SalaIds = dto.SalaIds ?? new List<string>(),
                UsuarioEspacioIds = dto.UsuarioEspacioIds ?? new List<string>(),
                PeticionIds = dto.PeticionIds ?? new List<string>(),
                InvitacionIds = dto.InvitacionIds ?? new List<string>()
            };
        }

        public static EspacioDto ToDto(EspacioPersist persist)
        {
            return new EspacioDto
            {
                Id_Espacio = persist.Id_Espacio,
                Nombre = persist.Nombre,
                Direccion = persist.Direccion,
                SalaIds = persist.SalaIds ?? new List<string>(),
                UsuarioEspacioIds = persist.UsuarioEspacioIds ?? new List<string>(),
                PeticionIds = persist.PeticionIds ?? new List<string>(),
                InvitacionIds = persist.InvitacionIds ?? new List<string>()
            };
        }
    }
}