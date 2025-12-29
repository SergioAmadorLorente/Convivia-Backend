using Convivia.Application.Mappers;
using Convivia.Shared.Services;
using Convivia.Shared.DTOs;
using Convivia.Application.Repositories;
using Convivia.Domain.Entities;
using MapsterMapper;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Convivia.Application.Services
{
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

            if (!string.IsNullOrWhiteSpace(espacioid))
            {
                plantillaTarea.EspacioId = espacioid;
            }

            if (string.IsNullOrWhiteSpace(plantillaTarea.TimeZoneId))
            {
                plantillaTarea.TimeZoneId = TimeZoneInfo.Local.Id;
            }

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

            var plantilla = await GetByEspacioAndIdAsync(espacioid, id);
            if (plantilla == null)
                throw new ArgumentException($"La plantilla con id '{id}' no existe en el espacio '{espacioid}'.", nameof(id));

            if (!string.IsNullOrWhiteSpace(dto.Nombre) && string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("Nombre no puede estar vac�o si se proporciona.", nameof(dto.Nombre));

            if (dto.HoraLimite.HasValue)
            {
            }

            if (dto.karma.HasValue && dto.karma < 0)
                throw new ArgumentException("Karma no puede ser negativo.", nameof(dto.karma));

            if (dto.GracePeriodMinutes.HasValue && (dto.GracePeriodMinutes < 1 || dto.GracePeriodMinutes > 60))
                throw new ArgumentException("GracePeriodMinutes debe estar entre 1 y 60 minutos (m�ximo 1 hora).", nameof(dto.GracePeriodMinutes));

            if (dto.DiasRepeticion != null && dto.DiasRepeticion.Count > 0)
            {
                var diasUnicos = new HashSet<int>();
                foreach (int dia in dto.DiasRepeticion)
                {
                    if (dia < 0 || dia > 6)
                        throw new ArgumentException("DiasRepeticion debe contener valores entre 0 y 6 (0=Domingo, 6=S�bado).", nameof(dto.DiasRepeticion));
                    if (!diasUnicos.Add(dia))
                        throw new ArgumentException("DiasRepeticion contiene valores duplicados.", nameof(dto.DiasRepeticion));
                }
            }

            if (dto.FechaFin.HasValue)
            {
            }

            var domPlantilla = plantilla.Adapt<PlantillaTarea>();

            var diasAnterior = domPlantilla.DiasRepeticion ?? new List<int>();
            var diasNuevo = dto.DiasRepeticion ?? diasAnterior;
            var diasRemovidos = diasAnterior.Except(diasNuevo).ToList();
            var DiasAnadidos = diasNuevo.Except(diasAnterior).ToList();
            bool diasRepeticionCambiaron = diasRemovidos.Any() || DiasAnadidos.Any();

            if (!string.IsNullOrWhiteSpace(dto.Nombre))
                domPlantilla.Nombre = dto.Nombre;

            if (!string.IsNullOrWhiteSpace(dto.Descripcion))
                domPlantilla.Descripcion = dto.Descripcion;

            if (dto.karma.HasValue)
                domPlantilla.karma = dto.karma.Value;

            if (dto.GracePeriodMinutes.HasValue)
                domPlantilla.GracePeriodMinutes = dto.GracePeriodMinutes.Value;

            if (dto.FechaFin.HasValue)
                domPlantilla.EndDate = DateOnly.FromDateTime(dto.FechaFin.Value);

            if (dto.DiasRepeticion != null && dto.DiasRepeticion.Count >= 0)
                domPlantilla.DiasRepeticion = dto.DiasRepeticion;

            if (diasRepeticionCambiaron && domPlantilla.TareasId != null)
            {
                await SyncronizarTareasConDiasRepeticion(id, domPlantilla, diasRemovidos, DiasAnadidos, dto.UsuariosAsignacion, dto.HoraLimite);
            }

            await _repository.UpdateAsync(id, domPlantilla);

            return _mapper.Map<PlantillaTareaDto>(domPlantilla);
        }

        private async Task SyncronizarTareasConDiasRepeticion(
            string plantillaId,
            PlantillaTarea plantilla,
            List<int> diasRemovidos,
            List<int> DiasAnadidos,
            List<string>? usuariosAsignacion,
            TimeOnly? horaLimiteForNewTasks)
        {
            try
            {
                foreach (int diaRemovido in diasRemovidos)
                {
                    var tareasAEliminar = new List<string>();
                    foreach (var tareaId in plantilla.TareasId ?? new List<string>())
                    {
                        var tarea = await _tareaRepository.GetInstanciaAsync(plantillaId, tareaId);
                        if (tarea != null && tarea.DiaSemana == diaRemovido)
                        {
                            tareasAEliminar.Add(tareaId);
                            await _tareaRepository.DeleteAsync(tareaId);
                            _logger.LogInformation("Tarea {TareaId} eliminada (d�a {Dia} removido)", tareaId, diaRemovido);
                        }
                    }
                    plantilla.TareasId = plantilla.TareasId.Except(tareasAEliminar).ToList();
                }

                if (DiasAnadidos.Any())
                {
                    if (usuariosAsignacion == null || usuariosAsignacion.Count == 0)
                    {
                        throw new InvalidOperationException(
                            $"Se agregaron nuevos d�as ({string.Join(",", DiasAnadidos)}) pero no se proporcion� UsuariosAsignacion. " +
                            "Debe asignar al menos un usuario para crear tareas en estos d�as.");
                    }

                    TimeOnly? horaToUse = horaLimiteForNewTasks;
                    if (!horaToUse.HasValue)
                    {
                        foreach (var existingTareaId in plantilla.TareasId ?? new List<string>())
                        {
                            var existingTarea = await _tareaRepository.GetInstanciaAsync(plantillaId, existingTareaId);
                            if (existingTarea != null && existingTarea.HoraLimite.HasValue)
                            {
                                horaToUse = existingTarea.HoraLimite;
                                break;
                            }
                        }
                    }

                    if (!horaToUse.HasValue)
                    {
                        throw new InvalidOperationException("Se agregaron nuevos d�as de repetici�n, se requiere HoraLimite para crear las instancias de tarea o debe existir al menos una tarea previa con HoraLimite.");
                    }

                    foreach (int DiaAnadido in DiasAnadidos)
                    {
                        var nuevaTarea = new Tarea
                        {
                            Id = Guid.NewGuid().ToString(),
                            PlantillaId = plantillaId,
                            DiaSemana = DiaAnadido,
                            Estado = TareaEstado.Pendiente,
                            FechaLimite = null,
                            HoraLimite = horaToUse,
                            UsuarioEspacioId = usuariosAsignacion.Count == 1 
                                ? usuariosAsignacion[0] 
                                : usuariosAsignacion[Math.Min(DiasAnadidos.IndexOf(DiaAnadido), usuariosAsignacion.Count - 1)]
                        };

                        await _tareaRepository.AddAsync(nuevaTarea);
                        plantilla.TareasId.Add(nuevaTarea.Id);

                        _logger.LogInformation(
                            "Tarea {TareaId} creada para d�a {Dia} con usuario {UsuarioId}",
                            nuevaTarea.Id, DiaAnadido, nuevaTarea.UsuarioEspacioId);
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

            var plantilla = await _repository.GetByEspacioAndIdAsync(espacioid, id);
            if (plantilla == null)
                return false;

            if (plantilla.TareasId != null && plantilla.TareasId.Count > 0)
            {
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

        private static bool IsOverdue(Tarea tarea, PlantillaTarea plantilla)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(plantilla.TimeZoneId);
            var nowUtc = DateTime.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);

            if (tarea.DiaSemana != (int)nowLocal.DayOfWeek)
                return false;

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