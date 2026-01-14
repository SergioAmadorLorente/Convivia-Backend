using Convivia.Application.Repositories;
using Convivia.Domain.Entities;
using Convivia.Infrastructure.HostedServices;
using Convivia.Infrastructure.Queues;
using Convivia.Infrastructure.Repositories;
using Convivia.Infrastructure.Services;
using Convivia.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Convivia.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Servicios de infraestructura
            services.AddScoped<IFirebaseService, FirebaseService>();
            services.AddSingleton<IErrorQueue>(sp => new InMemoryErrorQueue(capacity: 1000));
            services.AddScoped<FirestoreErrorRepository>(); 
            services.AddHostedService<ErrorWriterBackgroundService>();

            // Repositorios concretos
            services.AddScoped<IInvitacionRepository, InvitacionRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IEspacioRepository, EspacioRepository>();
            services.AddScoped<IPeticionRepository, PeticionRepository>();
            services.AddScoped<IPlantillaTareaRepository, PlantillaTareaRepository>();
            services.AddScoped<ITareaRepository, TareaRepository>();
            services.AddScoped<IPermisoRepository, PermisoRepository>();
            services.AddScoped<IRolRepository, RolRepository>();
            services.AddScoped<IReservaRepository, ReservaRepository>();
            services.AddScoped<IUsuarioEspacioRepository, UsuarioEspacioRepository>();
            services.AddScoped<IFacturaRepository, FacturaRepository>();

            return services;
        }
    }
}