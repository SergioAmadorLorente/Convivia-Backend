using Mapster;


namespace Convivia.Application.Mappers.Config
{
    /// <summary>
    /// Provides Mapster configuration for mapping between domain entities and DTOs.
    /// </summary>
    public static class MapsterConfig
    {

        /// <summary>
        /// Registers mapping configurations between an entity and its related DTOs.
        /// </summary>
        /// <typeparam name="TEntidad">The domain entity type.</typeparam>
        /// <param name="config">The Mapster TypeAdapterConfig instance.</param>
        public static void RegisterPair<TEntidad, TDto, TCreateDto, TUpdateDto>(TypeAdapterConfig config)
        {
            // Entity <-> DTO
            config.NewConfig<TEntidad, TDto>();
            config.NewConfig<TDto, TEntidad>();

            // Create DTO <-> Entity
            config.NewConfig<TCreateDto, TEntidad>();
            config.NewConfig<TEntidad, TCreateDto>();

            // Update DTO -> Entity (ignore nulls to avoid overwriting)
            config.NewConfig<TUpdateDto, TEntidad>()
                  .IgnoreNullValues(true);
            // Entity -> Update DTO
            config.NewConfig<TEntidad, TUpdateDto>();
        }

        /// <summary>
        /// Registers a simple pair mapping between an entity and a DTO.
        /// </summary>
        public static void RegisterPair<TEntidad, TDto>(TypeAdapterConfig config)
        {
            config.NewConfig<TEntidad, TDto>();
            config.NewConfig<TDto, TEntidad>();
        }
    }
}