using Convivia.Application.DTOs;
using Convivia.Domain.Models;
using Convivia.Infrastructure.Models;
using Mapster;

namespace Convivia.Application.Mappings;

public class TareaMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Tarea, FirestoreTarea>()
    .Map(dest => dest.FechaLimite,
         src => DateTime.SpecifyKind(src.FechaLimite, DateTimeKind.Utc))
    .Map(dest => dest.Prorroga,
         src => src.Prorroga.HasValue
             ? DateTime.SpecifyKind(src.Prorroga.Value, DateTimeKind.Utc)
             : (DateTime?)null);

    }
}