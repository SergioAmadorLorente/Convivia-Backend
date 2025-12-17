using Mapster;
using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using System;

namespace Convivia.Infrastructure.Mappers
{
    /// <summary>
    /// Convertidor global para Mapster: TipoRol/string <-> Rol
    /// Permite que Mapster convierta automáticamente entre el enum TipoRol y objetos Rol
    /// </summary>
    public class RolTypeConverter : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // TipoRol -> Rol
            config.ForType<TipoRol, Rol>().MapWith(src => MapTipoRolToRol(src));

            // string -> Rol (mantenido para compatibilidad con mapeos legacy)
            config.ForType<string, Rol>().MapWith(src => MapStringToRol(src));

            // Rol -> TipoRol
            config.ForType<Rol, TipoRol>().MapWith(src => 
                src != null && src.Nombre != null && src.Nombre.Equals("Admin", StringComparison.OrdinalIgnoreCase) 
                    ? TipoRol.Admin 
                    : TipoRol.Usuario);

            // Rol -> string (nombre)
            config.ForType<Rol, string>().MapWith(src => src != null ? src.Nombre ?? string.Empty : string.Empty);
        }

        private static Rol MapTipoRolToRol(TipoRol tipoRol)
        {
            var rol = new Rol();
            if (tipoRol == TipoRol.Admin)
            {
                rol.SetConfiguracionAdmin();
            }
            else
            {
                rol.SetConfiguracionUsuario();
            }

            rol.Nombre = tipoRol.ToString();
            return rol;
        }

        private static Rol MapStringToRol(string src)
        {
            if (string.IsNullOrWhiteSpace(src))
            {
                return new Rol();
            }

            var rol = new Rol();
            if (src.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                rol.SetConfiguracionAdmin();
            }
            else
            {
                rol.SetConfiguracionUsuario();
            }

            rol.Nombre = src;
            return rol;
        }
    }
}
