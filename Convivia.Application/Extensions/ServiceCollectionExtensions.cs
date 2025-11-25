using Convivia.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Convivia.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Registrar servicios de aplicación
            // Si tienes interfaz IInvitacionService: services.AddScoped<IInvitacionService, InvitacionService>();
            services.AddScoped<InvitacionService>();
            services.AddScoped<UsuarioService>();

            // Registrar mappers, validators, MediatR, etc. si procede
            // services.AddAutoMapper(typeof(YourProfile).Assembly);

            return services;
        }
    }
}