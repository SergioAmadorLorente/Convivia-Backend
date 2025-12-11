using Mapster;
using Convivia.Domain.Entities;
using Convivia.Infrastructure.Models;
using Convivia.Shared.DTOs;

namespace Convivia.Infrastructure.Mappers
{
    /// <summary>
    /// Mapster configuration for Permiso mappings between Domain and Persistence layers
    /// El mapeo de Rol (string <-> objeto) se gestiona automáticamente por RolTypeConverter
    /// Los mapeos PermisoDto <-> Permiso están configurados en MapsterBootstrap (Application layer)
    /// </summary>
    public class PermisoMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // FireStorePermiso -> Permiso (Domain)
            // El mapeo de Rol se aplica automáticamente gracias al convertidor global
            config.NewConfig<FireStorePermiso, Permiso>();

            // Permiso (Domain) -> FireStorePermiso
            // El mapeo de Rol se aplica automáticamente gracias al convertidor global
            config.NewConfig<Permiso, FireStorePermiso>();

            // NOTA: Los mapeos PermisoDto <-> Permiso están en MapsterBootstrap.cs
            // para evitar conflictos con la expansión de propiedades del Rol
        }
    }
}
