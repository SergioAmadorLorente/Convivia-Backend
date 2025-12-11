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
            if (string.IsNullOrWhiteSpace(src))
            {
                Console.WriteLine("[RolTypeConverter] Entrada vacía, retornando Rol vacío");
                return new Rol();
            }

            var rol = new Rol();
            if (src.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                rol.SetConfigurarcionAdmin();
                Console.WriteLine($"[RolTypeConverter] Configurado Admin - CrearTarea: {rol.CrearTarea}, EliminarTarea: {rol.EliminarTarea}");
            }
            else
            {
                rol.SetConfigurarcionUsuario();
                Console.WriteLine($"[RolTypeConverter] Configurado Usuario - CrearTarea: {rol.CrearTarea}, EliminarTarea: {rol.EliminarTarea}");
            }
            return rol;
        }
    }
}
