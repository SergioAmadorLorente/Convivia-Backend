using Convivia.Application.DTOs;
using Convivia.Domain.Models;
using Convivia.Infrastructure.Models;
using Mapster;

namespace Convivia.Infrastructure.Mappings;
public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        RegisterPair<PlantillaTarea, FirestorePlantillaTarea>(config);
        RegisterPair<Tarea, FirestoreTarea>(config);
    }

    private void RegisterPair<TEntidad, TFirestore>(TypeAdapterConfig config)
    {
        config.NewConfig<TEntidad, TFirestore>();
        config.NewConfig<TFirestore, TEntidad>();
    }
}