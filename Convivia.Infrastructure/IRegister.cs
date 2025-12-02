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

            // Sala 
            config.NewConfig<FireStoreSala, Sala>();
            config.NewConfig<Sala, FireStoreSala>();

        }

    }
}