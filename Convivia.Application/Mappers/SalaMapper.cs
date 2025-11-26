using System;
using Mapster;
using Convivia.Shared.DTOs;
using Convivia.Domain.Entities;
using Convivia.Application.Mappers;

namespace Convivia.Application.Mappers
{
    public class SalaMapper
    {
        // Registrar las configuraciones de Mapster una sola vez al cargar la clase.
        static SalaMapper()
        {
            // Asegura que las reglas genéricas (incluyendo IgnoreNullValues para Update DTO) estén registradas.
            MapsterConfig.RegisterPair<Sala, SalaDto, CreateSalaDto, UpdateSalaDto>(TypeAdapterConfig.GlobalSettings);
        }
    }
}
