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
            config.ForType<Rol, TipoRol>().MapWith(src => MapRolToTipoRol(src));

            // Rol -> string (nombre)
            config.ForType<Rol, string>().MapWith(src => src != null ? src.Nombre ?? string.Empty : string.Empty);
        }

        private static Rol MapTipoRolToRol(TipoRol tipoRol)
        {
            var rol = new Rol();
            
            switch (tipoRol)
            {
                case TipoRol.Admin:
                    rol.SetConfiguracionAdmin();
                    break;
                case TipoRol.Moderador:
                    rol.SetConfiguracionModerador();
                    break;
                case TipoRol.Usuario:
                default:
                    rol.SetConfiguracionUsuario();
                    break;
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
            else if (src.Equals("Moderador", StringComparison.OrdinalIgnoreCase))
            {
                rol.SetConfiguracionModerador();
            }
            else
            {
                rol.SetConfiguracionUsuario();
            }

            rol.Nombre = src;
            return rol;
        }

        private static TipoRol MapRolToTipoRol(Rol src)
        {
            if (src == null || string.IsNullOrWhiteSpace(src.Nombre))
                return TipoRol.Usuario;

            if (src.Nombre.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return TipoRol.Admin;
            
            if (src.Nombre.Equals("Moderador", StringComparison.OrdinalIgnoreCase))
                return TipoRol.Moderador;
            
            return TipoRol.Usuario;
        }
    }
}
