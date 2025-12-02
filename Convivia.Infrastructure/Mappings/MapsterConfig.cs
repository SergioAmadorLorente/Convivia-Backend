using Convivia.Application.DTOs;
using Convivia.Domain.Entities;
using Convivia.Infrastructure.Models;
using Mapster;

namespace Convivia.Infrastructure;
public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        RegisterPair<PlantillaTarea, FirestorePlantillaTarea>(config);
        RegisterPair<Usuario, FireStoreUsuario>(config);
        RegisterPair<Invitacion, FireStoreInvitacion>(config);
        RegisterPair<Sala, FireStoreSala>(config);
    }

    private void RegisterPair<TEntidad, TFirestore>(TypeAdapterConfig config)
    {
        config.NewConfig<TEntidad, TFirestore>();
        config.NewConfig<TFirestore, TEntidad>();
    }
}