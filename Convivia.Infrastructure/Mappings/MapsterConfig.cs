using Mapster;
using Convivia.Application.DTOs;
using Convivia.Domain.Models;
using System.Reflection.Metadata;

namespace Plantilla.Infraestructure.Mappings
{
    public class MapsterConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // De Entidad → DTO
            config.NewConfig<Sala, SalaDto>();

            // De DTO → Entidad
            config.NewConfig<SalaDto, Sala>();
        }
    }
}
