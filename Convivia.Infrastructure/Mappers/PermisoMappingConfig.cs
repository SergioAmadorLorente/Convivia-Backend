using Mapster;
using Convivia.Domain.Entities;
using Convivia.Infrastructure.Models;
using Convivia.Shared.DTOs;

namespace Convivia.Infrastructure.Mappers
{
    /// <summary>
    /// Mapster configuration for Permiso mappings between Domain and Persistence layers
    /// Expande las propiedades del objeto Rol anidado a propiedades planas en Firestore
    /// </summary>
    public class PermisoMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // FireStorePermiso -> Permiso (Domain)
            // Mapear desde propiedades planas de Firestore al objeto Rol anidado
            config.NewConfig<FireStorePermiso, Permiso>()
                .AfterMapping((src, dest) =>
                {
                    if (dest.Rol == null)
                        dest.Rol = new Rol();
                    
                    // Copiar propiedades planas de Firestore al Rol anidado
                    dest.Rol.Nombre = src.Rol;
                    dest.Rol.CrearTarea = src.CrearTarea;
                    dest.Rol.EliminarTarea = src.EliminarTarea;
                    dest.Rol.EditarTarea = src.EditarTarea;
                    dest.Rol.AñadirUsuario = src.AñadirUsuario;
                    dest.Rol.EliminarUsuario = src.EliminarUsuario;
                    dest.Rol.AsignarTarea = src.AsignarTarea;
                    dest.Rol.AsignarseTarea = src.AsignarseTarea;
                    dest.Rol.EliminarResidencia = src.EliminarResidencia;
                });

            // Permiso (Domain) -> FireStorePermiso
            // Expandir las propiedades del Rol anidado a propiedades planas de Firestore
            config.NewConfig<Permiso, FireStorePermiso>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Rol, src => src.Rol != null ? src.Rol.Nombre : "Usuario")
                .Map(dest => dest.CrearTarea, src => src.Rol != null && src.Rol.CrearTarea)
                .Map(dest => dest.EliminarTarea, src => src.Rol != null && src.Rol.EliminarTarea)
                .Map(dest => dest.EditarTarea, src => src.Rol != null && src.Rol.EditarTarea)
                .Map(dest => dest.AñadirUsuario, src => src.Rol != null && src.Rol.AñadirUsuario)
                .Map(dest => dest.EliminarUsuario, src => src.Rol != null && src.Rol.EliminarUsuario)
                .Map(dest => dest.AsignarTarea, src => src.Rol != null && src.Rol.AsignarTarea)
                .Map(dest => dest.AsignarseTarea, src => src.Rol != null && src.Rol.AsignarseTarea)
                .Map(dest => dest.EliminarResidencia, src => src.Rol != null && src.Rol.EliminarResidencia);
        }
    }
}
