using Mapster;
using Convivia.Infrastructure.Models; 
using Convivia.Domain.Entities;        

namespace Convivia.Infrastructure.Mappers
{
    public class InfraPersistenceRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Usuario
            config.NewConfig<FireStoreUsuario, Usuario>();
            config.NewConfig<Usuario, FireStoreUsuario>();

            // Espacio
            config.NewConfig<FireStoreEspacio, Espacio>();
            config.NewConfig<Espacio, FireStoreEspacio>();

            // Sala 
            config.NewConfig<FireStoreSala, Sala>();
            config.NewConfig<Sala, FireStoreSala>();

            // Invitacion
            config.NewConfig<FireStoreInvitacion, Invitacion>();
            config.NewConfig<Invitacion, FireStoreInvitacion>();

            // Peticion
            config.NewConfig<FireStorePeticion, Peticion>();
            config.NewConfig<Peticion, FireStorePeticion>();
            
            // PlantillaTarea
            config.NewConfig<FirestorePlantillaTarea, PlantillaTarea>();
            config.NewConfig<PlantillaTarea, FirestorePlantillaTarea>();

            // Tarea
            config.NewConfig<FirestoreTarea, Tarea>();
            config.NewConfig<Tarea, FirestoreTarea>();

            // UsuarioEspacio
            config.NewConfig<FireStoreUsuarioEspacio, UsuarioEspacio>();
            config.NewConfig<UsuarioEspacio, FireStoreUsuarioEspacio>();
        }
    }
}