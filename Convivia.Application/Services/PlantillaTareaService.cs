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
    public class PlantillaTareaService
    {
        private readonly IPlantillaTareaRepository _repository;
        private readonly IMapper _mapper;
        private readonly ITareaRepository _tareaRepository;
        private readonly ILogger<PlantillaTareaService> _logger;

        public PlantillaTareaService(
            IPlantillaTareaRepository plantilla,
            IMapper mapper,
            ITareaRepository tareaRepository,
            ILogger<PlantillaTareaService> logger)
        {
            _repository = plantilla ?? throw new ArgumentNullException(nameof(plantilla));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _tareaRepository = tareaRepository ?? throw new ArgumentNullException(nameof(tareaRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ============ CREATE ============

        public async Task<string> AddAsync(CreatePlantillaTareaDto dto, string espacioid)
        {
            if (dto == null) 
                throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(espacioid)) 
                throw new ArgumentNullException(nameof(espacioid));

            var plantilla = _mapper.Map<PlantillaTarea>(dto);
            plantilla.EspacioId = espacioid;
            plantilla.TimeZoneId = plantilla.TimeZoneId ?? "Europe/Madrid";
            plantilla.TareasId ??= new List<string>();
            plantilla.EsPuntual = (dto.DiasRepeticion == null || dto.DiasRepeticion.Count == 0);

            var plantillaId = await _repository.AddAsync(plantilla);
            return plantillaId;
        }

        // ============ READ ============

        public async Task<PlantillaTareaDto?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) 
                throw new ArgumentNullException(nameof(id));

            var plantilla = await _repository.GetByIdAsync(id);
            return plantilla == null ? null : _mapper.Map<PlantillaTareaDto>(plantilla);
        }

        public async Task<PlantillaTareaDto?> GetByEspacioAndIdAsync(string espacioid, string id)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) 
                throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(id)) 
                throw new ArgumentNullException(nameof(id));

            var plantilla = await _repository.GetByEspacioAndIdAsync(espacioid, id);
            return plantilla == null ? null : _mapper.Map<PlantillaTareaDto>(plantilla);
        }

        public async Task<IEnumerable<PlantillaTareaDto>> GetAllAsync()
        {
            throw new NotImplementedException("Use GetAllByEspacioAsync instead. PlantillaTarea requires espacioId.");
        }

        public async Task<IEnumerable<PlantillaTareaDto>> GetAllByEspacioAsync(string espacioid)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) 
                throw new ArgumentNullException(nameof(espacioid));

            var plantillas = await _repository.GetAllByEspacioAsync(espacioid);
            return _mapper.Map<IEnumerable<PlantillaTareaDto>>(plantillas);
        }

        // ============ UPDATE ============

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
                return null;

            ValidateUpdateDto(dto);

            var domPlantilla = plantilla.Adapt<PlantillaTarea>();
            var diasAnterior = domPlantilla.DiasRepeticion ?? new List<int>();
            var diasNuevo = dto.DiasRepeticion ?? diasAnterior;

            ApplyUpdates(domPlantilla, dto);

            bool diasCambiaron = !diasAnterior.SequenceEqual(diasNuevo);
            if (diasCambiaron)
            {
                await SyncronizarTareasConDiasRepeticion(id, domPlantilla, diasAnterior, diasNuevo, dto.UsuariosAsignacion, dto.HoraLimite);
            }

            await _repository.UpdateAsync(id, domPlantilla);
            return _mapper.Map<PlantillaTareaDto>(domPlantilla);
        }

        public async Task UpdateTareasIdsAsync(string espacioid, string plantillaId, List<string> tareasIds)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(plantillaId))
                throw new ArgumentNullException(nameof(plantillaId));
            if (tareasIds == null)
                throw new ArgumentNullException(nameof(tareasIds));

            var updates = new Dictionary<string, object>
            {
                { "TareasId", tareasIds }
            };

            await _repository.UpdateAsync(espacioid, plantillaId, updates, useSetMerge: true);
        }

        // ============ DELETE ============

        public async Task<bool> DeleteAsync(string espacioid, string id)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) 
                throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(id)) 
                throw new ArgumentNullException(nameof(id));

            var plantilla = await _repository.GetByEspacioAndIdAsync(espacioid, id);
            if (plantilla == null)
                return false;

            await _repository.DeleteAsync(espacioid, id);
            return true;
        }

        // ============ HELPERS ============

        private void ValidateUpdateDto(UpdatePlantillaTareaDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.Nombre) && string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("Nombre no puede estar vacío si se proporciona.", nameof(dto.Nombre));

            if (dto.karma.HasValue && dto.karma < 0)
                throw new ArgumentException("Karma no puede ser negativo.", nameof(dto.karma));

            if (dto.DiasRepeticion != null && dto.DiasRepeticion.Count > 0)
            {
                var diasUnicos = new HashSet<int>();
                foreach (int dia in dto.DiasRepeticion)
                {
                    if (dia < 0 || dia > 6)
                        throw new ArgumentException("DiasRepeticion debe contener valores entre 0 y 6 (0=Domingo, 6=Sábado).", nameof(dto.DiasRepeticion));
                    if (!diasUnicos.Add(dia))
                        throw new ArgumentException("DiasRepeticion contiene valores duplicados.", nameof(dto.DiasRepeticion));
                }
            }
        }

        private void ApplyUpdates(PlantillaTarea plantilla, UpdatePlantillaTareaDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.Nombre))
                plantilla.Nombre = dto.Nombre;

            if (!string.IsNullOrWhiteSpace(dto.Descripcion))
                plantilla.Descripcion = dto.Descripcion;

            if (dto.karma.HasValue)
                plantilla.karma = dto.karma.Value;

            if (dto.FechaLimite.HasValue)
                plantilla.FechaLimite = dto.FechaLimite.Value;

            if (dto.DiasRepeticion != null)
                plantilla.DiasRepeticion = dto.DiasRepeticion;

            plantilla.EsPuntual = (plantilla.DiasRepeticion == null || plantilla.DiasRepeticion.Count == 0);
        }

        private async Task SyncronizarTareasConDiasRepeticion(
            string plantillaId,
            PlantillaTarea plantilla,
            List<int> diasAnterior,
            List<int> diasNuevo,
            List<string>? usuariosAsignacion,
            TimeOnly? horaLimiteNueva)
        {
            try
            {
                var diasRemovidos = diasAnterior.Except(diasNuevo).ToList();
                var diasAnadidos = diasNuevo.Except(diasAnterior).ToList();

                await EliminarTareasDelDias(plantillaId, plantilla, diasRemovidos);
                await CrearOActualizarTareas(plantillaId, plantilla, diasNuevo, diasAnadidos, usuariosAsignacion, horaLimiteNueva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sincronizando tareas con cambios en DiasRepeticion");
                throw;
            }
        }

        private async Task EliminarTareasDelDias(string plantillaId, PlantillaTarea plantilla, List<int> diasRemovidos)
        {
            foreach (int diaRemovido in diasRemovidos)
            {
                var tareasAEliminar = new List<string>();

                foreach (var tareaId in plantilla.TareasId ?? new List<string>())
                {
                    var tarea = await _tareaRepository.GetInstanciaAsync(plantilla.EspacioId, plantillaId, tareaId);
                    if (tarea != null && tarea.DiaSemana == diaRemovido)
                    {
                        await _tareaRepository.DeleteAsync(tareaId);
                        tareasAEliminar.Add(tareaId);
                        _logger.LogInformation("Tarea {TareaId} eliminada (día {Dia} removido)", tareaId, diaRemovido);
                    }
                }

                plantilla.TareasId = plantilla.TareasId.Except(tareasAEliminar).ToList();
            }
        }

        private async Task CrearOActualizarTareas(
            string plantillaId,
            PlantillaTarea plantilla,
            List<int> diasNuevo,
            List<int> diasAnadidos,
            List<string>? usuariosAsignacion,
            TimeOnly? horaLimiteNueva)
        {
            // Validar que no haya más usuarios que días nuevos
            if (usuariosAsignacion != null && usuariosAsignacion.Count > diasAnadidos.Count)
                throw new ArgumentException($"No puede haber más usuarios ({usuariosAsignacion.Count}) que días de repetición ({diasAnadidos.Count}).", nameof(usuariosAsignacion));

            TimeOnly? horaToUse = horaLimiteNueva;
            if (!horaToUse.HasValue)
            {
                horaToUse = await ObtenerHoraDelimiteDePrimeratarea(plantillaId, plantilla);
            }

            if (diasAnadidos.Any() && !horaToUse.HasValue)
                throw new InvalidOperationException("Se agregaron nuevos días de repetición, se requiere HoraLimite para crear las instancias de tarea o debe existir al menos una tarea previa con HoraLimite.");

            // Procesar cada día nuevo
            for (int i = 0; i < diasAnadidos.Count; i++)
            {
                int dia = diasAnadidos[i];
                
                // Asignación lineal: si hay usuario en esta posición, asignarlo; sino, null
                string? usuarioAsignado = i < (usuariosAsignacion?.Count ?? 0) ? usuariosAsignacion![i] : null;

                var tareaExistente = await BuscarTareaDelDia(plantillaId, plantilla, dia);

                if (tareaExistente != null)
                {
                    // Actualizar usuario si se proporcionaron usuarios
                    if (usuariosAsignacion != null && usuariosAsignacion.Count > 0)
                    {
                        tareaExistente.UsuarioEspacioId = usuarioAsignado;
                        await _tareaRepository.UpdateAsync(tareaExistente.Id, tareaExistente, merge: true);
                        _logger.LogInformation("Tarea {TareaId} del día {Dia} actualizada con usuario {UsuarioId}", 
                            tareaExistente.Id, dia, usuarioAsignado ?? "sin asignar");
                    }
                }
                else
                {
                    var nuevaTarea = new Tarea
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        PlantillaId = plantillaId,
                        DiaSemana = dia,
                        Estado = TareaEstado.Pendiente,
                        HoraLimite = horaToUse,
                        UsuarioEspacioId = usuarioAsignado
                    };

                    await _tareaRepository.AddAsync(plantilla.EspacioId, nuevaTarea);
                    plantilla.TareasId.Add(nuevaTarea.Id);
                    _logger.LogInformation("Tarea {TareaId} creada para día {Dia} con usuario {UsuarioId}", 
                        nuevaTarea.Id, dia, usuarioAsignado ?? "sin asignar");
                }
            }
        }

        private async Task<TimeOnly?> ObtenerHoraDelimiteDePrimeratarea(string plantillaId, PlantillaTarea plantilla)
        {
            foreach (var tareaId in plantilla.TareasId ?? new List<string>())
            {
                var tarea = await _tareaRepository.GetInstanciaAsync(plantilla.EspacioId, plantillaId, tareaId);
                if (tarea != null && tarea.HoraLimite.HasValue)
                    return tarea.HoraLimite;
            }
            return null;
        }

        private async Task<Tarea?> BuscarTareaDelDia(string plantillaId, PlantillaTarea plantilla, int dia)
        {
            foreach (var tareaId in plantilla.TareasId ?? new List<string>())
            {
                var tarea = await _tareaRepository.GetInstanciaAsync(plantilla.EspacioId, plantillaId, tareaId);
                if (tarea != null && tarea.DiaSemana == dia)
                    return tarea;
            }
            return null;
        }
    }
}