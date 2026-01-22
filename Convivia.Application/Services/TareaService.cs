using Convivia.Domain.Entities;
using Convivia.Application.Repositories;
using Convivia.Shared.DTOs;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Application.Services
{
    public class TareaService
    {
        private readonly ITareaRepository _tareaRepository;
        private readonly IMapper _mapper;
        private readonly PlantillaTareaService _ptservice;
        private readonly IUsuarioEspacioRepository _usuarioEspacioRepository;
        private readonly ILogger<TareaService> _logger;

        private static readonly int[] KARMAS_VALIDOS = { 5, 15, 25, 50 };

        public TareaService(
            ITareaRepository tareaRepository,
            IMapper mapper,
            PlantillaTareaService ptservice,
            IUsuarioEspacioRepository usuarioEspacioRepository,
            ILogger<TareaService> logger)
        {
            _tareaRepository = tareaRepository ?? throw new ArgumentNullException(nameof(tareaRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _ptservice = ptservice ?? throw new ArgumentNullException(nameof(ptservice));
            _usuarioEspacioRepository = usuarioEspacioRepository ?? throw new ArgumentNullException(nameof(usuarioEspacioRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ============ CREATE ============

        public async Task<string> AddAsync(string espacioid, CreateTareaDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));

            ValidateCreateTareaDto(dto);

            bool esPuntual = dto.DiasRepeticion == null || dto.DiasRepeticion.Count == 0;

            if (esPuntual && !dto.FechaLimite.HasValue)
                throw new ArgumentException("FechaLimite es obligatoria para tareas puntuales.", nameof(dto.FechaLimite));

            if (esPuntual && dto.UsuariosAsignacion != null && dto.UsuariosAsignacion.Count > 1)
                throw new ArgumentException("Una tarea puntual no puede tener más de un usuario asignado.", nameof(dto.UsuariosAsignacion));

            var createPlantilla = _mapper.Map<CreatePlantillaTareaDto>(dto);
            createPlantilla.DiasRepeticion = dto.DiasRepeticion ?? new List<int>();
            createPlantilla.UsuariosAsignacion = dto.UsuariosAsignacion ?? new List<string>();

            var tareas = esPuntual 
                ? CrearTareasPuntuales(dto) 
                : CrearTareasRepetidas(dto, createPlantilla.DiasRepeticion);

            // Crear la plantilla primero sin tareas
            var plantillaId = await _ptservice.AddAsync(createPlantilla, espacioid);

            // Luego crear las tareas con el ID de la plantilla
            foreach (var tarea in tareas)
            {
                tarea.PlantillaId = plantillaId;
            }

            var idsCreadas = await _tareaRepository.AddAsyncList(tareas);

            // Actualizar la plantilla para agregar los IDs de tareas creadas
            if (idsCreadas != null && idsCreadas.Count > 0)
            {
                await _ptservice.UpdateTareasIdsAsync(espacioid, plantillaId, idsCreadas);
            }

            return plantillaId;
        }

        // ============ READ ============

        public async Task<TareaDto?> GetByEspacioAndPlantillaAndTareaAsync(string espacioid, string plantillaId, string tareaId)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new ArgumentNullException(nameof(plantillaId));
            if (string.IsNullOrWhiteSpace(tareaId)) throw new ArgumentNullException(nameof(tareaId));

            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, plantillaId);
            if (plantilla == null) return null;

            var tarea = await _tareaRepository.GetInstanciaAsync(plantillaId, tareaId);
            if (tarea == null) return null;

            var pt = plantilla.Adapt<PlantillaTarea>();
            await UpdateToOverdueIfNeededAsync(tarea, pt);

            return MapTareaToDto(tarea, plantilla, pt);
        }

        public async Task<IEnumerable<TareaDto>> GetByDiaAndEstadoAsync(string espacioid, int diaSemana, TareaEstado estado)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));
            if (diaSemana < 0 || diaSemana > 6) throw new ArgumentException("Día debe estar entre 0 y 6.", nameof(diaSemana));

            return await FilterByMultipleCriteriaAsync(espacioid, diaSemana, estado, null, null);
        }

        public async Task<IEnumerable<TareaDto>> GetByEstadoAsync(string espacioid, TareaEstado estado)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));

            return await FilterByMultipleCriteriaAsync(espacioid, null, estado, null, null);
        }

        public async Task<IEnumerable<TareaDto>> FilterAsync(string espacioid, int? diaSemana = null, string? estado = null, string? usuarioId = null, string? plantillaId = null)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));
            if (diaSemana.HasValue && (diaSemana < -1 || diaSemana > 6)) throw new ArgumentException("diaSemana debe estar entre -1 y 6.");

            TareaEstado? parsedEstado = null;
            if (!string.IsNullOrWhiteSpace(estado))
            {
                if (!Enum.TryParse<TareaEstado>(estado, ignoreCase: true, out var p))
                    throw new ArgumentException("estado no válido. Valores válidos: Pendiente, FueraDePlazo, Completada, CompletadaFueradePlazo");
                parsedEstado = p;
            }

            return await FilterByMultipleCriteriaAsync(espacioid, diaSemana, parsedEstado, usuarioId, plantillaId);
        }

        public async Task<IEnumerable<PlantillaTareaDto>> GetAllByEspacioAsync(string espacioid)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));

            var pttareas = await _ptservice.GetAllByEspacioAsync(espacioid);
            return _mapper.Map<IEnumerable<PlantillaTareaDto>>(pttareas);
        }

        public async Task<PlantillaTareaDto> GetByEspacioAndIdAsync(string espacioid, string id)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var pttarea = await _ptservice.GetByEspacioAndIdAsync(espacioid, id);
            if (pttarea == null) throw new ArgumentException("La plantilla no existe o no pertenece al espacio especificado.", nameof(id));
            
            return _mapper.Map<PlantillaTareaDto>(pttarea);
        }

        // ============ UPDATE ============

        public async Task<TareaDto> UpdateAsync(string espacioid, string plantillaid, string tareaid, UpdateTareaDto dto)
        {
            return await UpdatePartialAsync(espacioid, plantillaid, tareaid, dto) ?? throw new InvalidOperationException("No se pudo actualizar la tarea.");
        }

        public async Task<TareaDto?> UpdatePartialAsync(string espacioid, string plantillaid, string tareaid, UpdateTareaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(plantillaid)) throw new ArgumentNullException(nameof(plantillaid));
            if (string.IsNullOrWhiteSpace(tareaid)) throw new ArgumentNullException(nameof(tareaid));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, plantillaid);
            if (plantilla == null) return null;

            var existing = await _tareaRepository.GetInstanciaAsync(plantillaid, tareaid, ct);
            if (existing == null) return null;

            var domPlantilla = plantilla.Adapt<PlantillaTarea>();
            
            // Actualizar a overdue si es necesario antes de procesar cambios de estado
            await UpdateToOverdueIfNeededAsync(existing, domPlantilla);
            
            ProcessEstadoUpdate(existing, domPlantilla, dto, ct);

            var updates = ObtenerActualizacionesDesdeDto(dto);
            if (updates.Count > 0)
            {
                await _tareaRepository.UpdateAsync(tareaid, updates, useSetMerge: false, ct);
            }

            var updated = await _tareaRepository.GetInstanciaAsync(plantillaid, tareaid, ct);
            return updated == null ? null : MapTareaToDto(updated, plantilla, domPlantilla);
        }

        public async Task<TareaDto?> UpdateMergeAsync(string espacioid, string plantillaid, string tareaid, UpdateTareaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(plantillaid)) throw new ArgumentNullException(nameof(plantillaid));
            if (string.IsNullOrWhiteSpace(tareaid)) throw new ArgumentNullException(nameof(tareaid));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, plantillaid);
            if (plantilla == null) return null;

            var existing = await _tareaRepository.GetInstanciaAsync(plantillaid, tareaid, ct);
            if (existing == null) return null;

            var domPlantilla = plantilla.Adapt<PlantillaTarea>();
            
            // Actualizar a overdue si es necesario antes de procesar cambios de estado
            await UpdateToOverdueIfNeededAsync(existing, domPlantilla);
            
            ProcessEstadoUpdate(existing, domPlantilla, dto, ct);

            _mapper.Map(dto, existing);
            if (!string.IsNullOrWhiteSpace(dto.Estado) && Enum.TryParse<TareaEstado>(dto.Estado, ignoreCase: true, out var estado))
                existing.Estado = estado;

            CalcularProrroga(existing, domPlantilla);

            await _tareaRepository.UpdateAsync(tareaid, existing, merge: true, ct);

            var updated = await _tareaRepository.GetInstanciaAsync(plantillaid, tareaid, ct);
            return updated == null ? null : MapTareaToDto(updated, plantilla, domPlantilla);
        }

        public async Task<TareaDto?> UpdateCompleteAsync(string espacioid, string plantillaid, string tareaid, UpdateTareaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(plantillaid)) throw new ArgumentNullException(nameof(plantillaid));
            if (string.IsNullOrWhiteSpace(tareaid)) throw new ArgumentNullException(nameof(tareaid));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, plantillaid);
            if (plantilla == null) return null;

            var existing = await _tareaRepository.GetInstanciaAsync(plantillaid, tareaid, ct);
            if (existing == null) return null;

            var domPlantilla = plantilla.Adapt<PlantillaTarea>();
            
            // Actualizar a overdue si es necesario antes de procesar cambios de estado
            await UpdateToOverdueIfNeededAsync(existing, domPlantilla);
            
            await ProcessEstadoUpdate(existing, domPlantilla, dto, ct);

            var domain = _mapper.Map<Tarea>(dto);
            domain.Id = tareaid;
            domain.PlantillaId = plantillaid;
            if (!string.IsNullOrWhiteSpace(dto.Estado) && Enum.TryParse<TareaEstado>(dto.Estado, ignoreCase: true, out var estado))
                domain.Estado = estado;

            CalcularProrroga(domain, domPlantilla);

            await _tareaRepository.UpdateAsync(tareaid, domain, merge: false, ct);

            var updated = await _tareaRepository.GetInstanciaAsync(plantillaid, tareaid, ct);
            return updated == null ? null : MapTareaToDto(updated, plantilla, domPlantilla);
        }

        // ============ DELETE ============

        public async Task<bool> DeleteAsync(string espacioid, string plantillaid, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(plantillaid)) throw new ArgumentNullException(nameof(plantillaid));

            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, plantillaid);
            if (plantilla == null) return false;

            if (plantilla.TareasId != null && plantilla.TareasId.Count > 0)
            {
                foreach (string tareaid in plantilla.TareasId)
                {
                    if (!string.IsNullOrWhiteSpace(tareaid))
                    {
                        try
                        {
                            await _tareaRepository.DeleteAsync(tareaid, ct);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException($"Error eliminando tarea {tareaid} de plantilla {plantillaid}.", ex);
                        }
                    }
                }
            }

            var result = await _ptservice.DeleteAsync(espacioid, plantillaid);
            if (!result) throw new InvalidOperationException($"No se pudo eliminar la plantilla {plantillaid}.");

            return true;
        }

        // ============ HELPERS ============

        private void ValidateCreateTareaDto(CreateTareaDto dto)
        {
            if (!KARMAS_VALIDOS.Contains(dto.karma))
                throw new ArgumentException("karma debe ser 5, 15, 25 o 50.", nameof(dto.karma));

            if (dto.HoraLimite == default(TimeOnly))
                throw new ArgumentException("HoraLimite es obligatoria para todas las tareas.", nameof(dto.HoraLimite));

            if (dto.DiasRepeticion != null && dto.DiasRepeticion.Count > 0)
            {
                var diasUnicos = new HashSet<int>();
                foreach (int dia in dto.DiasRepeticion)
                {
                    if (dia < 0 || dia > 6)
                        throw new ArgumentException("DiasRepeticion debe contener valores entre 0 y 6 (0=Lunes, 6=Domingo).", nameof(dto.DiasRepeticion));
                    if (!diasUnicos.Add(dia))
                        throw new ArgumentException("DiasRepeticion contiene valores duplicados.", nameof(dto.DiasRepeticion));
                }
            }
        }

        private List<Tarea> CrearTareasPuntuales(CreateTareaDto dto)
        {
            var tarea = _mapper.Map<Tarea>(dto);
            tarea.DiaSemana = -1;
            tarea.Estado = TareaEstado.Pendiente;
            tarea.HoraLimite = dto.HoraLimite;
            tarea.UsuarioEspacioId = dto.UsuariosAsignacion != null && dto.UsuariosAsignacion.Count > 0 
                ? dto.UsuariosAsignacion[0] 
                : null;

            return new List<Tarea> { tarea };
        }

        private List<Tarea> CrearTareasRepetidas(CreateTareaDto dto, List<int> diasRepeticion)
        {
            var tareas = new List<Tarea>();
            var users = dto.UsuariosAsignacion ?? new List<string>();

            // Validar que no haya más usuarios que días
            if (users.Count > diasRepeticion.Count)
                throw new ArgumentException($"No puede haber más usuarios ({users.Count}) que días de repetición ({diasRepeticion.Count}).", nameof(dto.UsuariosAsignacion));

            for (int i = 0; i < diasRepeticion.Count; i++)
            {
                var tarea = _mapper.Map<Tarea>(dto);
                tarea.DiaSemana = diasRepeticion[i];
                tarea.Estado = TareaEstado.Pendiente;
                tarea.HoraLimite = dto.HoraLimite;
                
                // Asignación lineal: si hay usuario en esta posición, asignarlo; sino, null
                tarea.UsuarioEspacioId = i < users.Count ? users[i] : null;

                tareas.Add(tarea);
            }

            return tareas;
        }

        private List<int> ConvertirDiasDelCliente(List<int> diasCliente)
        {
            if (diasCliente == null || diasCliente.Count == 0) return new List<int>();
            // Los días ya vienen en formato 0-6, no necesitan conversión
            return diasCliente;
        }

        private async Task<IEnumerable<TareaDto>> FilterByMultipleCriteriaAsync(
            string espacioid,
            int? diaSemana,
            TareaEstado? estado,
            string? usuarioId,
            string? plantillaId)
        {
            var pttareas = await _ptservice.GetAllByEspacioAsync(espacioid);
            var tareas = new List<TareaDto>();

            foreach (var plantilla in pttareas)
            {
                // If a specific plantillaId filter was provided, skip other plantillas
                if (!string.IsNullOrWhiteSpace(plantillaId) && !string.Equals(plantilla.Id, plantillaId, StringComparison.OrdinalIgnoreCase))
                    continue;
                bool plantillaActiva = IsPlantillaActive(plantilla);
                bool esRepetida = plantilla.DiasRepeticion != null && plantilla.DiasRepeticion.Count > 0;

                if (esRepetida && !plantillaActiva) continue;

                if (plantillaActiva)
                    await ResetCompletedRepeatedTasksAsync(plantilla);

                var pt = plantilla.Adapt<PlantillaTarea>();

                foreach (var tareaId in plantilla.TareasId ?? new List<string>())
                {
                    var tarea = await _tareaRepository.GetInstanciaAsync(plantilla.Id, tareaId);
                    if (tarea == null) continue;

                    // Actualizar a overdue si es necesario antes de filtrar
                    if (!IsOverdue(tarea, pt))
                        tarea.Estado = TareaEstado.Pendiente;

                    await UpdateToOverdueIfNeededAsync(tarea, pt);

                    if (diaSemana.HasValue && tarea.DiaSemana != diaSemana) continue;
                    if (estado.HasValue && !MatchesEstado(tarea, estado.Value)) continue;
                    if (!string.IsNullOrWhiteSpace(usuarioId) && tarea.UsuarioEspacioId != usuarioId) continue;

                    tareas.Add(MapTareaToDto(tarea, plantilla, pt));
                }
            }

            return tareas;
        }

        private TareaDto MapTareaToDto(Tarea tarea, PlantillaTareaDto plantilla, PlantillaTarea domPlantilla)
        {
            var dto = _mapper.Map<TareaDto>(tarea);
            dto.Nombre = plantilla.Nombre;
            dto.Descripcion = plantilla.Descripcion;
            dto.karma = plantilla.karma;
            dto.HoraLimite = tarea.HoraLimite;
            dto.Estado = tarea.Estado.ToString();
            dto.Overdue = IsOverdue(tarea, domPlantilla);
            dto.EsPuntual = tarea.DiaSemana == -1;
            dto.UsuarioEspacioId = tarea.UsuarioEspacioId;
            return dto;
        }

        private bool MatchesEstado(Tarea tarea, TareaEstado estado)
        {
            return estado switch
            {
                TareaEstado.Completada => tarea.Estado == TareaEstado.Completada || tarea.Estado == TareaEstado.CompletadaFueradePlazo,
                TareaEstado.CompletadaFueradePlazo => tarea.Estado == TareaEstado.CompletadaFueradePlazo,
                TareaEstado.Pendiente => tarea.Estado == TareaEstado.Pendiente,
                TareaEstado.FueraDePlazo => tarea.Estado == TareaEstado.FueraDePlazo,
                _ => false
            };
        }

        private async Task ResetCompletedRepeatedTasksAsync(PlantillaTareaDto plantilla)
        {
            if (plantilla.DiasRepeticion == null || plantilla.DiasRepeticion.Count == 0) return;
            if (!IsPlantillaActive(plantilla)) return;

            try
            {
                foreach (var tareaId in plantilla.TareasId ?? new List<string>())
                {
                    var tarea = await _tareaRepository.GetInstanciaAsync(plantilla.Id, tareaId);
                    if (tarea == null || tarea.DiaSemana < 0 || tarea.Estado != TareaEstado.Completada) continue;

                    tarea.Estado = TareaEstado.Pendiente;
                    tarea.FechaRealizacion = null;
                    await _tareaRepository.UpdateAsync(tareaId, tarea, merge: true);
                    _logger.LogDebug("Tarea repetida {TareaId} reseteada a Pendiente.", tareaId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reseteando tareas completadas para plantilla {PlantillaId}", plantilla.Id);
            }
        }

        private static bool IsPlantillaActive(PlantillaTareaDto plantilla)
        {
            if (plantilla.DiasRepeticion == null || plantilla.DiasRepeticion.Count == 0) return true;
            if (!plantilla.FechaLimite.HasValue) return true;

            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
            return hoy <= plantilla.FechaLimite.Value;
        }

        private async Task UpdateToOverdueIfNeededAsync(Tarea tarea, PlantillaTarea plantilla)
        {
            if (tarea.Estado != TareaEstado.Pendiente || tarea.FechaRealizacion.HasValue) return;
            if (!IsOverdue(tarea, plantilla)) return;

            try
            {
                tarea.Estado = TareaEstado.FueraDePlazo;
                await _tareaRepository.UpdateAsync(tarea.Id, tarea, merge: true);
                _logger.LogDebug("Tarea {TareaId} actualizada a FueraDePlazo", tarea.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar tarea {TareaId} a FueraDePlazo", tarea.Id);
            }
        }

        private async Task ProcessEstadoUpdate(Tarea tarea, PlantillaTarea plantilla, UpdateTareaDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Estado)) return;

            if (!Enum.TryParse<TareaEstado>(dto.Estado, ignoreCase: true, out var parsedEstado))
                throw new ArgumentException($"Estado '{dto.Estado}' no válido.", nameof(dto.Estado));

            if (parsedEstado == TareaEstado.Pendiente && IsOverdueWithUpdates(tarea, plantilla, dto))
                throw new InvalidOperationException("No se puede marcar como pendiente una tarea que está overdue.");

            if (parsedEstado == TareaEstado.Completada && !dto.FechaRealizacion.HasValue)
                dto.FechaRealizacion = DateTime.UtcNow;

            bool esCompletar = parsedEstado == TareaEstado.Completada || parsedEstado == TareaEstado.CompletadaFueradePlazo;
            if (esCompletar && IsOverdueWithUpdates(tarea, plantilla, dto))
                parsedEstado = TareaEstado.CompletadaFueradePlazo;

            if (esCompletar && tarea.Estado != TareaEstado.Completada && tarea.Estado != TareaEstado.CompletadaFueradePlazo && !string.IsNullOrWhiteSpace(tarea.UsuarioEspacioId))
            {
                await AwardKarmaToUserAsync(tarea.UsuarioEspacioId, plantilla.karma, ct);
            }
        }

        private void CalcularProrroga(Tarea tarea, PlantillaTarea plantilla)
        {
            if (tarea.Estado != TareaEstado.Completada && tarea.Estado != TareaEstado.CompletadaFueradePlazo) return;

            var dueUtc = GetDueUtcForTask(tarea, plantilla);
            var nowUtc = DateTime.UtcNow;

            if (dueUtc.HasValue && nowUtc > dueUtc.Value)
                tarea.Prorroga = nowUtc - dueUtc.Value;
        }

        private async Task AwardKarmaToUserAsync(string usuarioEspacioId, int karma, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(usuarioEspacioId) || karma <= 0) return;

            try
            {
                await _usuarioEspacioRepository.UpdateKarmaAsync(usuarioEspacioId, karma, ct);
                _logger.LogDebug("Karma {Karma} sumado al usuario {UsuarioId}", karma, usuarioEspacioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sumando karma al usuario {UsuarioId}", usuarioEspacioId);
            }
        }

        private IDictionary<string, object> ObtenerActualizacionesDesdeDto(UpdateTareaDto dto)
        {
            var updates = new Dictionary<string, object>();

            if (dto.FechaRealizacion.HasValue)
                updates["FechaRealizacion"] = dto.FechaRealizacion.Value;

            if (dto.Prorroga.HasValue)
                updates["ProrrogaSegundos"] = dto.Prorroga.Value.TotalSeconds;

            if (dto.HoraLimite.HasValue)
                updates["HoraLimite"] = dto.HoraLimite.Value.ToString("HH:mm");

            if (!string.IsNullOrWhiteSpace(dto.Estado) && Enum.TryParse<TareaEstado>(dto.Estado, ignoreCase: true, out var parsed))
                updates["Estado"] = parsed.ToString();

            if (!string.IsNullOrWhiteSpace(dto.UsuarioEspacioId))
                updates["UsuarioEspacioId"] = dto.UsuarioEspacioId;

            return updates;
        }

        private static bool IsOverdue(Tarea tarea, PlantillaTarea plantilla)
        {
            if (tarea.FechaRealizacion.HasValue) return false;

            var tz = TimeZoneInfo.FindSystemTimeZoneById(plantilla.TimeZoneId);
            var nowUtc = DateTime.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);
            var todayLocal = DateOnly.FromDateTime(nowLocal);

            if (tarea.DiaSemana == -1)
            {
                if (!plantilla.FechaLimite.HasValue || !tarea.HoraLimite.HasValue) return false;

                var fechaLimite = plantilla.FechaLimite.Value;
                if (todayLocal < fechaLimite) return false;
                if (todayLocal > fechaLimite) return true;

                var dueLocal = new DateTime(fechaLimite.Year, fechaLimite.Month, fechaLimite.Day,
                    tarea.HoraLimite.Value.Hour, tarea.HoraLimite.Value.Minute, 0, DateTimeKind.Unspecified);
                var dueUtc = new DateTimeOffset(dueLocal, tz.GetUtcOffset(dueLocal)).UtcDateTime;

                return nowUtc >= dueUtc;
            }

            if (!tarea.HoraLimite.HasValue) return false;

            if (plantilla.FechaLimite.HasValue && todayLocal > plantilla.FechaLimite.Value) return true;

            int currentDay = ((int)nowLocal.DayOfWeek - 1 + 7) % 7;
            int daysDiff = tarea.DiaSemana - currentDay;
            
            // Si daysDiff es negativo, el día ya pasó esta semana, se puede overdue
            // Si daysDiff es positivo, el día aún no llega esta semana, no es overdue
            // Si daysDiff es 0, es hoy, revisar hora
            
            if (daysDiff > 0) return false;  // Día aún no ha llegado esta semana
            
            if (daysDiff < 0) return true;   // Día ya pasó esta semana, está overdue
            
            // daysDiff == 0: es hoy, verificar hora
            return nowLocal.Hour > tarea.HoraLimite.Value.Hour ||
                   (nowLocal.Hour == tarea.HoraLimite.Value.Hour && nowLocal.Minute >= tarea.HoraLimite.Value.Minute);
        }

        private static DateTime? GetDueUtcForTask(Tarea tarea, PlantillaTarea plantilla)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(plantilla.TimeZoneId);
            var nowUtc = DateTime.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);

            if (tarea.DiaSemana == -1)
            {
                if (!plantilla.FechaLimite.HasValue || !tarea.HoraLimite.HasValue) return null;

                var dueLocal = new DateTime(plantilla.FechaLimite.Value.Year, plantilla.FechaLimite.Value.Month,
                    plantilla.FechaLimite.Value.Day, tarea.HoraLimite.Value.Hour, tarea.HoraLimite.Value.Minute,
                    0, DateTimeKind.Unspecified);
                return new DateTimeOffset(dueLocal, tz.GetUtcOffset(dueLocal)).UtcDateTime;
            }

            if (!tarea.HoraLimite.HasValue) return null;

            if (plantilla.FechaLimite.HasValue)
            {
                var todayLocal = DateOnly.FromDateTime(nowLocal);
                if (todayLocal > plantilla.FechaLimite.Value) return null;
            }

            int currentDay = ((int)nowLocal.DayOfWeek - 1 + 7) % 7;
            int daysDiff = tarea.DiaSemana - currentDay;
            
            // Si daysDiff es negativo, el día ya pasó, será la próxima semana
            if (daysDiff <= 0)
                daysDiff += 7;

            var occurrenceDate = nowLocal.Date.AddDays(daysDiff);
            var occurrenceDateOnly = DateOnly.FromDateTime(occurrenceDate);

            if (plantilla.FechaLimite.HasValue && occurrenceDateOnly > plantilla.FechaLimite.Value) return null;

            var dueLocalRep = new DateTime(occurrenceDate.Year, occurrenceDate.Month, occurrenceDate.Day,
                tarea.HoraLimite.Value.Hour, tarea.HoraLimite.Value.Minute, 0, DateTimeKind.Unspecified);
            return new DateTimeOffset(dueLocalRep, tz.GetUtcOffset(dueLocalRep)).UtcDateTime;
        }

        private static bool IsOverdueWithUpdates(Tarea tarea, PlantillaTarea plantilla, UpdateTareaDto? dto)
        {
            if (dto == null) return IsOverdue(tarea, plantilla);

            var temp = tarea.Adapt<Tarea>();
            if (dto.Prorroga.HasValue) temp.Prorroga = dto.Prorroga.Value;
            if (dto.FechaRealizacion.HasValue) temp.FechaRealizacion = dto.FechaRealizacion.Value;

            return IsOverdue(temp, plantilla);
        }
    }
}