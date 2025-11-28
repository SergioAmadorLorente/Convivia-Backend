using Convivia.Application.DTOs;
using Convivia.Domain.Models;
using Mapster;

namespace Convivia.Application.Mappers;
public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        RegisterPair<PlantillaTarea, PlantillaTareaDto>(config);
        RegisterPair<CreatePlantillaTareaDto, TareaDto>(config);
    }

    private void RegisterPair<TEntidad, TDto>(TypeAdapterConfig config)
    {
        config.NewConfig<TEntidad, TDto>();
        config.NewConfig<TDto, TEntidad>();
    }

}