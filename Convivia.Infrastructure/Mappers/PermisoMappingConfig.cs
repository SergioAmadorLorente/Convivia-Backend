using Mapster;
using Convivia.Domain.Entities;
using Convivia.Infrastructure.Models;
using Convivia.Shared.DTOs;

namespace Convivia.Infrastructure.Mappers
{
    /// <summary>
    /// Mapster configuration for Permiso mappings between Domain, DTO and Persistence layers
    /// El mapeo de Rol (string <-> objeto) se gestiona automáticamente por RolTypeConverter
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

            // PermisoDto -> Permiso (Domain)
            // El mapeo de Rol se aplica automáticamente gracias al convertidor global
            config.NewConfig<PermisoDto, Permiso>();

            // Permiso (Domain) -> PermisoDto
            // El mapeo de Rol se aplica automáticamente gracias al convertidor global
            config.NewConfig<Permiso, PermisoDto>();
        }
    }
}
