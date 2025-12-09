using Mapster;
using Convivia.Domain.Entities;
using System;

namespace Convivia.Infrastructure.Mappers
{
    /// <summary>
    /// Convertidor global para Mapster: string <-> Rol
    /// Permite que Mapster convierta automáticamente entre nombres de rol (string) y objetos Rol
    /// </summary>
    public class RolTypeConverter : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // string -> Rol
            config.ForType<string, Rol>().MapWith(src => MapStringToRol(src));

            // Rol -> string (nombre)
            config.ForType<Rol, string>().MapWith(src => src != null ? src.Nombre ?? string.Empty : string.Empty);
        }

        private static Rol MapStringToRol(string src)
        {
            if (string.IsNullOrWhiteSpace(src)) return new Rol();

            var rol = new Rol();
            if (src.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                rol.SetConfigurarcionAdmin();
            }
            else
            {
                rol.SetConfigurarcionUsuario();
            }
            return rol;
        }
    }
}
