using Mapster;
using Convivia.Infrastructure.Models;
using Convivia.Domain.Entities;
using Convivia.Shared.Contracts;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Convivia.Infrastructure.Mappers
{
    public class InfraPersistenceRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Usuario
            config.NewConfig<FireStoreUsuario, Usuario>();
            config.NewConfig<Usuario, FireStoreUsuario>();

            // Espacio
            config.NewConfig<FireStoreEspacio, Espacio>();
            config.NewConfig<Espacio, FireStoreEspacio>();

            // Sala 
            config.NewConfig<FireStoreSala, Sala>();
            config.NewConfig<Sala, FireStoreSala>();

            // Invitacion
            config.NewConfig<FireStoreInvitacion, Invitacion>();
            config.NewConfig<Invitacion, FireStoreInvitacion>();

            // Peticion
            config.NewConfig<FireStorePeticion, Peticion>();
            config.NewConfig<Peticion, FireStorePeticion>();

            // PlantillaTarea
            config.NewConfig<FirestorePlantillaTarea, PlantillaTarea>();
            config.NewConfig<PlantillaTarea, FirestorePlantillaTarea>();

            // Tarea
            config.NewConfig<FirestoreTarea, Tarea>();
            config.NewConfig<Tarea, FirestoreTarea>();

            // Reserva
            config.NewConfig<FireStoreReserva, Reserva>();
            config.NewConfig<Reserva, FireStoreReserva>();

            // UsuarioEspacio
            config.NewConfig<FireStoreUsuarioEspacio, UsuarioEspacio>();
            config.NewConfig<UsuarioEspacio, FireStoreUsuarioEspacio>();


            // Factura
            config.NewConfig<FireStoreFactura, Factura>()
                .Map(dest => dest.Id, src => src.Id);
            config.NewConfig<Factura, FireStoreFactura>()
                .Map(dest => dest.Id, src => src.Id);

            // ErrorRecord persistence mapping (DTO <-> Firestore model)
            config.NewConfig<FireStoreErrorRecord, ErrorRecordDto>();
            config.NewConfig<ErrorRecordDto, FireStoreErrorRecord>()
                .Map(dest => dest.CorrelationId, src => src.CorrelationId ?? string.Empty)
                .Map(dest => dest.TimestampUtc, src => src.TimestampUtc == default ? DateTime.UtcNow : src.TimestampUtc)
                // Convertir ValidationErrors (IReadOnlyDictionary) a Dictionary para Firestore
                .Map(dest => dest.ValidationErrors, src => src.ValidationErrors == null ? null : src.ValidationErrors.ToDictionary(kv => kv.Key, kv => kv.Value))
                .Map(dest => dest.Entity, src => src.Entity)
                .Map(dest => dest.Key, src => src.Key);
        }

    }
}
