using Mapster;
using System.Reflection;
using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using Convivia.Shared.Contracts;
using System;
using System.Collections.Generic;

namespace Convivia.Application.Mappers
{
    public static class MapsterBootstrap
    {
        public static void Configure(TypeAdapterConfig config)
        {
            // IMPORTANTE: Escanear primero Infrastructure para cargar RolTypeConverter
            // antes de configurar los mapeos que lo necesitan
            try
            {
                var infraAssembly = Assembly.Load("Convivia.Infrastructure");
                config.Scan(infraAssembly);
            }
            catch
            {
                // ignore if assembly not available during design-time operations
            }

            // Register application-level mappings (DTO <-> Domain)

            // Espacio mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<Espacio, EspacioDto, CreateEspacioDto, UpdateEspacioDto>(config);

            // Sala mappings
            Config.MapsterConfig.RegisterPair<Sala, SalaDto, CreateSalaDto, UpdateSalaDto>(config);

            // Usuario mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<Usuario, UsuarioDto, CreateUsuarioDto, UpdateUsuarioDto>(config);

            // Rol mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<Rol, RolDto, CreateRolDto, UpdateRolDto>(config);

            // Configuración personalizada para Permiso: CreatePermisoDto -> Permiso
            // Mapster convierte TipoRol -> Rol automáticamente usando RolTypeConverter
            config.NewConfig<CreatePermisoDto, Permiso>()
                .Map(dest => dest.Rol, src => src.Rol); // RolTypeConverter maneja TipoRol -> Rol automáticamente

            // Configuración personalizada: Permiso -> PermisoDto
            // Expandir las propiedades del Rol en el DTO para facilitar consumo en API
            config.NewConfig<Permiso, PermisoDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Rol, src => MapRolToTipoRol(src.Rol))
                .Map(dest => dest, src => src.Rol); // Mapster copia automáticamente propiedades coincidentes

            // Invitacion mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<Invitacion, InvitacionDto, CreateInvitacionDto, UpdateInvitacionDto>(config);

            // Peticion mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<Peticion, PeticionDto, CreatePeticionDto, UpdatePeticionDto>(config);

            // Tarea mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<Tarea, TareaDto, CreateTareaDto, UpdateTareaDto>(config);

            // PlantillaTarea mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<PlantillaTarea, PlantillaTareaDto, CreatePlantillaTareaDto, UpdatePlantillaTareaDto>(config);

            Config.MapsterConfig.RegisterPair<Reserva, ReservaDto, CreateReservaDto, UpdateReservaDto>(config);
            // UsuarioEspacio mappings (DTO <-> Domain)
            Config.MapsterConfig.RegisterPair<UsuarioEspacio, UsuarioEspacioDto, CreateUsuarioEspacioDto, UpdateUsuarioEspacioDto>(config);


            // Factura mappings 
            Config.MapsterConfig.RegisterPair<Factura, FacturaDto, CreateFacturaDto, UpdateFacturaDto>(config);

            // Error mappings (Contract <-> ContractDto)
            config.NewConfig<ErrorRecord, ErrorRecordDto>()
                .Map(dest => dest.CorrelationId, src => src.CorrelationId ?? string.Empty)
                .Map(dest => dest.TraceId, src => src.TraceId)
                .Map(dest => dest.Status, src => src.Status)
                .Map(dest => dest.Message, src => src.Message)
                .Map(dest => dest.Route, src => src.Route)
                .Map(dest => dest.TimestampUtc, src => src.TimestampUtc == default ? DateTime.UtcNow : src.TimestampUtc)
                .Map(dest => dest.Stack, src => src.Stack)
                .Map(dest => dest.ValidationErrors, src => src.ValidationErrors)
                .Map(dest => dest.Entity, src => src.Entity)
                .Map(dest => dest.Key, src => src.Key);

            // Reverse mapping (DTO -> Contract)
            config.NewConfig<ErrorRecordDto, ErrorRecord>()
                .Map(dest => dest.CorrelationId, src => src.CorrelationId ?? string.Empty)
                .Map(dest => dest.TraceId, src => src.TraceId)
                .Map(dest => dest.Status, src => src.Status)
                .Map(dest => dest.Message, src => src.Message)
                .Map(dest => dest.Route, src => src.Route)
                .Map(dest => dest.TimestampUtc, src => src.TimestampUtc == default ? DateTime.UtcNow : src.TimestampUtc)
                .Map(dest => dest.Stack, src => src.Stack)
                .Map(dest => dest.ValidationErrors, src => src.ValidationErrors)
                .Map(dest => dest.Entity, src => src.Entity)
                .Map(dest => dest.Key, src => src.Key);


            // Custom: map CreateTareaDto -> CreatePlantillaTareaDto
            config.NewConfig<CreateTareaDto, CreatePlantillaTareaDto>()
                .Map(dest => dest.Nombre, src => src.Nombre)
                .Map(dest => dest.Descripcion, src => src.Descripcion)
                .Map(dest => dest.karma, src => src.karma)
                .Map(dest => dest.DiasRepeticion, src => src.DiasRepeticion)
                .Map(dest => dest.FechaLimite, src => src.FechaLimite)
                .Map(dest => dest.UsuariosAsignacion, src => src.UsuariosAsignacion ?? new List<string>());

            // Custom: map CreateTareaDto -> Tarea (para manejar DateOnly?)
            config.NewConfig<CreateTareaDto, Tarea>()
                .Map(dest => dest.HoraLimite, src => src.HoraLimite)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.PlantillaId)
                .Ignore(dest => dest.DiaSemana)
                .Ignore(dest => dest.Estado)
                .Ignore(dest => dest.UsuarioEspacioId)
                .Ignore(dest => dest.FechaRealizacion);

            // Scan infrastructure assembly for IRegister implementations (Domain <-> Persistence)
            try
            {
                var infraAssembly = Assembly.Load("Convivia.Infrastructure");
                config.Scan(infraAssembly);
            }
            catch
            {
                // ignore if assembly not available during design-time operations
            }
        }

        /// <summary>
        /// Mapea un objeto Rol a TipoRol enum
        /// </summary>
        private static TipoRol MapRolToTipoRol(Rol? rol)
        {
            if (rol == null || string.IsNullOrWhiteSpace(rol.Nombre))
                return TipoRol.Usuario;

            if (rol.Nombre.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return TipoRol.Admin;

            if (rol.Nombre.Equals("Moderador", StringComparison.OrdinalIgnoreCase))
                return TipoRol.Moderador;

            return TipoRol.Usuario;
        }
    }
}
