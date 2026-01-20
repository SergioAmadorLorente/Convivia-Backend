using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Convivia.Infrastructure.HostedServices;
using Convivia.Infrastructure.Queues;
using Convivia.Infrastructure.Repositories;
using Convivia.Infrastructure.Services;
using Convivia.Shared.Services;
using Convivia.Application.Repositories; // si hace falta ajustar namespaces
using System;

namespace Convivia.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Servicios de infraestructura
            services.AddScoped<IFirebaseService, FirebaseService>();

            // Cola en memoria (singleton) - compartida entre middleware y hosted service
            services.AddSingleton<IErrorQueue>(sp => new InMemoryErrorQueue(capacity: 1000));

            // Repositorio de errores: interfaz + implementación
            services.AddSingleton<IErrorRepository, FirestoreErrorRepository>();

            // Hosted service que consume la cola
            services.AddHostedService<ErrorWriterBackgroundService>();

            // Repositorios concretos de la aplicación
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
