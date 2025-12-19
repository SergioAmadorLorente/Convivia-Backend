using Convivia.Application.Mappers;
using Convivia.Shared.Services;
using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;
using Convivia.Domain.Entities;
using MapsterMapper;
using Convivia.Domain.Repositories;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Convivia.Application.Services
{
    /// <summary>
    /// Modelo de respuesta para validaciones
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }

        public static ValidationResult Success() => new ValidationResult { IsValid = true };
        public static ValidationResult Failure(string message) => new ValidationResult { IsValid = false, ErrorMessage = message };
    }

    public class PlantillaTareaService
    {
        private readonly IPlantillaTareaRepository _repository;
        private readonly IMapper _mapper;
        private readonly ITareaRepository _tareaRepository;
        private readonly ILogger<PlantillaTareaService> _logger;

        public PlantillaTareaService(
            IPlantillaTareaRepository plantilla,
            IMapper _mapper,
            ITareaRepository tareaRepository,
            ILogger<PlantillaTareaService> logger)
        {
            _repository = plantilla ?? throw new ArgumentNullException(nameof(plantilla));
            this._mapper = _mapper ?? throw new ArgumentNullException(nameof(_mapper));
            _tareaRepository = tareaRepository ?? throw new ArgumentNullException(nameof(tareaRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(CreatePlantillaTareaDto dto, string espacioid)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var plantillaTarea = _mapper.Map<PlantillaTarea>(dto);

            // Asignar espacio y valores por defecto si faltan
            if (!string.IsNullOrWhiteSpace(espacioid))
            {
                plantillaTarea.EspacioId = espacioid;
            }

            // TimeZoneId no se recibe en el Create DTO -> poner valor por defecto si no está
            if (string.IsNullOrWhiteSpace(plantillaTarea.TimeZoneId))
            {
                plantillaTarea.TimeZoneId = TimeZoneInfo.Local.Id;
            }

            // Si FechaLimite no se proporcionó, dejarla null (tarea repetida sin límite temporal)

            // Asegurar lista TareasId no sea null
            plantillaTarea.TareasId ??= new List<string>();

            var plantillanovaid = await _repository.AddAsync(plantillaTarea);
            return plantillanovaid;
        }

        public async Task<PlantillaTareaDto> UpdateAsync(string espacioid, string id, UpdatePlantillaTareaDto dto)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Validación 1: Verificar que plantilla existe y pertenece al espacio
            var plantilla = await GetByEspacioAndIdAsync(espacioid, id);
            if (plantilla == null)
                throw new ArgumentException($"La plantilla con id '{id}' no existe en el espacio '{espacioid}'.", nameof(id));

            // Validación 2: Verificar Nombre si se actualiza
            if (!string.IsNullOrWhiteSpace(dto.Nombre) && string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("Nombre no puede estar vacío si se proporciona.", nameof(dto.Nombre));

            // Validación 3: Verificar HoraLimite si se actualiza (solo se usa para creación de nuevas tareas)
            if (dto.HoraLimite.HasValue)
            {
                // hora válida por el tipo TimeOnly
            }

            // Validación 4: Verificar FacturaId si se actualiza
            if (!string.IsNullOrWhiteSpace(dto.FacturaId))
            {
                var facturaValidation = await ValidateFacturaExistsAsync(dto.FacturaId);
                if (!facturaValidation.IsValid)
                    throw new InvalidOperationException(facturaValidation.ErrorMessage);
            }

            // Validación 5: Verificar karma si se actualiza
            if (dto.karma.HasValue && dto.karma < 0)
                throw new ArgumentException("Karma no puede ser negativo.", nameof(dto.karma));

            // Validación 6: Verificar TareasId si se actualiza
            if (dto.TareasId != null && dto.TareasId.Count > 0)
            {
                foreach (var tareaId in dto.TareasId)
                {
                    if (string.IsNullOrWhiteSpace(tareaId))
                        throw new ArgumentException("TareasId contiene elementos vacíos.", nameof(dto.TareasId));
                }
            }

            // Validación 7: Verificar DiasRepeticion si se actualiza
            if (dto.DiasRepeticion != null && dto.DiasRepeticion.Count > 0)
            {
                foreach (int dia in dto.DiasRepeticion)
                {
                    if (dia < 0 || dia > 6)
                        throw new ArgumentException("DiasRepeticion debe contener valores entre 0 y 6 (0=Domingo, 6=Sábado).", nameof(dto.DiasRepeticion));
                }
            }

            var domPlantilla = plantilla.Adapt<PlantillaTarea>();

            // Detectar cambios en DiasRepeticion (para luego eliminar/crear tareas)
            var diasAnterior = domPlantilla.DiasRepeticion ?? new List<int>();
            var diasNuevo = dto.DiasRepeticion ?? diasAnterior;
            var diasRemovidos = diasAnterior.Except(diasNuevo).ToList();
            var diasAñadidos = diasNuevo.Except(diasAnterior).ToList();
            bool diasRepeticionCambiaron = diasRemovidos.Any() || diasAñadidos.Any();

            // Aplicar cambios del DTO al dominio
            if (!string.IsNullOrWhiteSpace(dto.Nombre))
                domPlantilla.Nombre = dto.Nombre;

            // plantilla no almacena HoraLimite

            if (!string.IsNullOrWhiteSpace(dto.Descripcion))
                domPlantilla.Descripcion = dto.Descripcion;

            if (dto.karma.HasValue)
                domPlantilla.karma = dto.karma.Value;

            if (!string.IsNullOrWhiteSpace(dto.FacturaId))
                domPlantilla.FacturaId = dto.FacturaId;

            if (dto.TareasId != null && dto.TareasId.Count > 0)
                domPlantilla.TareasId = dto.TareasId;

            if (dto.DiasRepeticion != null && dto.DiasRepeticion.Count >= 0)
                domPlantilla.DiasRepeticion = dto.DiasRepeticion;

            // Si DiasRepeticion cambió, sincronizar tareas
            if (diasRepeticionCambiaron && domPlantilla.TareasId != null)
            {
                // Pass HoraLimite from DTO so new tasks get the provided hour; if null, Syncronizar will try to infer from existing tasks
                await SyncronizarTareasConDiasRepeticion(id, domPlantilla, diasRemovidos, diasAñadidos, dto.UsuariosAsignacion, dto.HoraLimite);
            }

            await _repository.UpdateAsync(id, domPlantilla);

            return _mapper.Map<PlantillaTareaDto>(domPlantilla);
        }

        /// <summary>
        /// Sincroniza las tareas con los cambios en DiasRepeticion.
        /// - Elimina tareas de días que se removieron
        /// - Mantiene tareas de días que se mantienen
        /// - Crea nuevas tareas para días que se añadieron (si se proporciona UsuariosAsignacion)
        /// </summary>
        private async Task SyncronizarTareasConDiasRepeticion(
            string plantillaId,
            PlantillaTarea plantilla,
            List<int> diasRemovidos,
            List<int> diasAñadidos,
            List<string>? usuariosAsignacion,
            TimeOnly? horaLimiteForNewTasks)
        {
            try
            {
                // 1. Eliminar tareas de días que se removieron
                foreach (int diaRemovido in diasRemovidos)
                {
                    var tareasAEliminar = new List<string>();
                    foreach (var tareaId in plantilla.TareasId ?? new List<string>())
                    {
                        var tarea = await _tareaRepository.GetAsync(plantillaId, tareaId);
                        if (tarea != null && tarea.DiaSemana == diaRemovido)
                        {
                            tareasAEliminar.Add(tareaId);
                            await _tareaRepository.DeleteAsync(tareaId);
                            _logger.LogInformation("Tarea {TareaId} eliminada (día {Dia} removido)", tareaId, diaRemovido);
                        }
                    }
                    plantilla.TareasId = plantilla.TareasId.Except(tareasAEliminar).ToList();
                }

                // 2. Crear tareas para días que se añadieron
                if (diasAñadidos.Any())
                {
                    // Validar que se proporciona UsuariosAsignacion para crear nuevas tareas
                    if (usuariosAsignacion == null || usuariosAsignacion.Count == 0)
                    {
                        throw new InvalidOperationException(
                            $"Se agregaron nuevos días ({string.Join(",", diasAñadidos)}) pero no se proporcionó UsuariosAsignacion. " +
                            "Debe asignar al menos un usuario para crear tareas en estos días.");
                    }

                    // If no hora provided, try to infer from an existing tarea in plantilla
                    TimeOnly? horaToUse = horaLimiteForNewTasks;
                    if (!horaToUse.HasValue)
                    {
                        foreach (var existingTareaId in plantilla.TareasId ?? new List<string>())
                        {
                            var existingTarea = await _tareaRepository.GetAsync(plantillaId, existingTareaId);
                            if (existingTarea != null && existingTarea.HoraLimite.HasValue)
                            {
                                horaToUse = existingTarea.HoraLimite;
                                break;
                            }
                        }
                    }

                    // Require horaToUse to be present (either provided or inferred)
                    if (!horaToUse.HasValue)
                    {
                        throw new InvalidOperationException("Se agregaron nuevos días de repetición, se requiere HoraLimite para crear las instancias de tarea o debe existir al menos una tarea previa con HoraLimite.");
                    }

                    foreach (int diaAñadido in diasAñadidos)
                    {
                        // Crear nueva tarea para este día
                        var nuevaTarea = new Tarea
                        {
                            Id = Guid.NewGuid().ToString(),
                            PlantillaId = plantillaId,
                            DiaSemana = diaAñadido,
                            Estado = TareaEstado.Pendiente,
                            FechaLimite = plantilla.EndDate?.ToDateTime(TimeOnly.MinValue),
                            HoraLimite = horaToUse,
                            // Si se proporciona un solo usuario, asignarlo a todas las tareas nuevas
                            // Si se proporcionan múltiples usuarios, usar el que corresponda por posición
                            UsuarioEspacioId = usuariosAsignacion.Count == 1 
                                ? usuariosAsignacion[0] 
                                : usuariosAsignacion[Math.Min(diasAñadidos.IndexOf(diaAñadido), usuariosAsignacion.Count - 1)]
                        };

                        await _tareaRepository.AddAsync(nuevaTarea);
                        plantilla.TareasId.Add(nuevaTarea.Id);

                        _logger.LogInformation(
                            "Tarea {TareaId} creada para día {Dia} con usuario {UsuarioId}",
                            nuevaTarea.Id, diaAñadido, nuevaTarea.UsuarioEspacioId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sincronizando tareas con cambios en DiasRepeticion");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string espacioid, string id)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            // Validación 1: Verificar que plantilla existe y pertenece al espacio
            var plantilla = await _repository.GetByEspacioAndIdAsync(espacioid, id);
            if (plantilla == null)
                return false;

            // Validación 2: Verificar que no hay referencias internas inconsistentes
            if (plantilla.TareasId != null && plantilla.TareasId.Count > 0)
            {
                // Solo log de advertencia, no bloquea el delete
                // (Las tareas se eliminarán en el servicio de Tarea)
            }

            await _repository.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<PlantillaTareaDto>> GetAllByEspacioAsync(string espacioid)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));

            var plantillas = await _repository.GetAllAsync();
            var filtered = plantillas.Where(p => p.EspacioId == espacioid);
            return _mapper.Map<IEnumerable<PlantillaTareaDto>>(filtered);
        }

        public async Task<IEnumerable<PlantillaTareaDto>> GetAsync()
        {
            var plantillas = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<PlantillaTareaDto>>(plantillas);
        }

        public async Task<PlantillaTareaDto?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            var plantilla = await _repository.GetByIdAsync(id);
            if (plantilla == null)
                return null;
            return _mapper.Map<PlantillaTareaDto>(plantilla);
        }

        public async Task<PlantillaTareaDto?> GetByEspacioAndIdAsync(string espacioid, string id)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            var plantilla = await _repository.GetByEspacioAndIdAsync(espacioid, id);
            if (plantilla == null)
                return null;
            return _mapper.Map<PlantillaTareaDto>(plantilla);
        }

        /// <summary>
        /// Valida que el Espacio existe
        /// </summary>
        public async Task<ValidationResult> ValidateEspacioExistsAsync(string espacioid)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                return ValidationResult.Failure("EspacioId no puede estar vacío.");

            // TODO: Implementar consulta a IEspacioRepository cuando esté disponible
            // Por ahora, solo validar que no es null/vacío
            // En una implementación completa, verificar que el Espacio existe en BD

            return ValidationResult.Success();
        }

        /// <summary>
        /// Valida que los UsuarioEspacios existen y pertenecen al Espacio
        /// </summary>
        public async Task<ValidationResult> ValidateUsuariosEspacioExistAsync(string espacioid, List<string> usuarioEspaciosIds)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                return ValidationResult.Failure("EspacioId no puede estar vacío.");

            if (usuarioEspaciosIds == null || usuarioEspaciosIds.Count == 0)
                return ValidationResult.Success();

            foreach (var ueId in usuarioEspaciosIds)
            {
                if (string.IsNullOrWhiteSpace(ueId))
                    return ValidationResult.Failure("UsuarioEspacioId contiene valores vacíos.");

                // TODO: Implementar consulta a IUsuarioEspacioRepository cuando esté disponible
                // Verificar que el UsuarioEspacio existe y pertenece al Espacio
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// Valida que el UsuarioEspacio existe
        /// </summary>
        public async Task<ValidationResult> ValidateUsuarioEspacioExistAsync(string espacioid, string usuarioEspacioId)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                return ValidationResult.Failure("EspacioId no puede estar vacío.");

            if (string.IsNullOrWhiteSpace(usuarioEspacioId))
                return ValidationResult.Failure("UsuarioEspacioId no puede estar vacío.");

            // TODO: Implementar consulta a IUsuarioEspacioRepository cuando esté disponible
            // Verificar que el UsuarioEspacio existe y pertenece al Espacio

            return ValidationResult.Success();
        }

        /// <summary>
        /// Valida que la Sala existe y pertenece al Espacio
        /// </summary>
        public async Task<ValidationResult> ValidateSalaExistsAsync(string espacioid, string salaId)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                return ValidationResult.Failure("EspacioId no puede estar vacío.");

            if (string.IsNullOrWhiteSpace(salaId))
                return ValidationResult.Failure("SalaId no puede estar vacío.");

            // TODO: Implementar consulta a ISalaRepository cuando esté disponible
            // Verificar que la Sala existe y pertenece al Espacio

            return ValidationResult.Success();
        }

        /// <summary>
        /// Valida que la Factura existe
        /// </summary>
        public async Task<ValidationResult> ValidateFacturaExistsAsync(string facturaId)
        {
            if (string.IsNullOrWhiteSpace(facturaId))
                return ValidationResult.Failure("FacturaId no puede estar vacío.");

            // TODO: Implementar consulta a IFacturaRepository cuando esté disponible
            // Verificar que la Factura existe

            return ValidationResult.Success();
        }

        private static bool IsOverdue(Tarea tarea, PlantillaTarea plantilla)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(plantilla.TimeZoneId);
            var nowUtc = DateTime.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);

            if (tarea.DiaSemana != (int)nowLocal.DayOfWeek)
                return false;

            // Use task-specific HoraLimite; if missing, cannot compute overdue => return false
            if (!tarea.HoraLimite.HasValue)
                return false;

            var occurrenceDate = nowLocal.Date;
            var horaLimite = tarea.HoraLimite.Value;

            var dueLocal = new DateTime(occurrenceDate.Year, occurrenceDate.Month, occurrenceDate.Day,
                                        horaLimite.Hour, horaLimite.Minute, 0, DateTimeKind.Unspecified);
            var dueUtc = new DateTimeOffset(dueLocal, tz.GetUtcOffset(dueLocal)).UtcDateTime;

            if (plantilla.GracePeriodMinutes.HasValue)
                dueUtc = dueUtc.AddMinutes(plantilla.GracePeriodMinutes.Value);

            return nowUtc >= dueUtc;
        }
    }
}