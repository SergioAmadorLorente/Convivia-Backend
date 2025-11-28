using Mapster;


namespace Convivia.Application.Mappers.Config
{
    /// <summary>
    /// Provides Mapster configuration for mapping between domain entities and DTOs.
    /// </summary>
    public class MapsterConfig
    {
        /// <summary>
        /// Registers mapping configurations between an entity and its related DTOs.
        /// </summary>
        /// <typeparam name="TEntidad">The domain entity type.</typeparam>
        /// <typeparam name="TDto">The main DTO type associated with the entity.</typeparam>
        /// <typeparam name="TCreateSalaDto">The DTO type used for creating a Sala entity.</typeparam>
        /// <typeparam name="TUpdateSalaDto">The DTO type used for updating a Sala entity.</typeparam>
        /// <param name="config">The Mapster TypeAdapterConfig instance.</param>
        public static void RegisterPair<TEntidad, TDto, TCreateSalaDto, TUpdateSalaDto>(TypeAdapterConfig config)
        {
            /// <summary>
            /// Maps from entity to DTO.
            /// </summary>
            config.NewConfig<TEntidad, TDto>();

            /// <summary>
            /// Maps from DTO to entity.
            /// </summary>
            config.NewConfig<TDto, TEntidad>();

            /// <summary>
            /// Maps from Create DTO to entity.
            /// </summary>
            config.NewConfig<TCreateSalaDto, TEntidad>();

            /// <summary>
            /// Maps from entity to Create DTO.
            /// </summary>
            config.NewConfig<TEntidad, TCreateSalaDto>();

            /// <summary>
            /// Maps from Update DTO to entity, ignoring null values.
            /// </summary>
            // Ignoring null values to prevent overwriting existing entity values with nulls from the DTO.
            config.NewConfig<TUpdateSalaDto, TEntidad>()
                  .IgnoreNullValues(true);

            /// <summary>
            /// Maps from entity to Update DTO.
            /// </summary>
            config.NewConfig<TEntidad, TUpdateSalaDto>();
        }
    }
}