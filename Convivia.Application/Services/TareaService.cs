using Convivia.Domain.Entities;
using Convivia.Domain.Repositories;
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
        private readonly ITareaRepository _repository;
        private readonly IMapper _mapper;
        private readonly PlantillaTareaService _ptservice;
        private readonly IUsuarioEspacioRepository _usuarioEspacioRepository;
        private readonly ILogger<TareaService> _logger;

        private static readonly int[] KarmasValidos = { 5, 15, 25, 50 };

        public TareaService(
            ITareaRepository tarea,
            IMapper _mapper,
            PlantillaTareaService ptservice,
            IUsuarioEspacioRepository usuarioEspacioRepository,
            ILogger<TareaService> logger)
        {
            _repository = tarea ?? throw new ArgumentNullException(nameof(tarea));
            this._mapper = _mapper ?? throw new ArgumentNullException(nameof(_mapper));
            _ptservice = ptservice ?? throw new ArgumentNullException(nameof(ptservice));
            _usuarioEspacioRepository = usuarioEspacioRepository ?? throw new ArgumentNullException(nameof(usuarioEspacioRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(string espacioid, CreateTareaDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));

            var plantillaValidation = await _ptservice.ValidateEspacioExistsAsync(espacioid);
            if (!plantillaValidation.IsValid)
                throw new InvalidOperationException(plantillaValidation.ErrorMessage);

            if (!KarmasValidos.Contains(dto.karma))
                throw new ArgumentException("karma debe ser 5, 15, 25 o 50.", nameof(dto.karma));

            bool esPuntual = dto.DiasRepeticion == null || dto.DiasRepeticion.Count == 0;

            if (!esPuntual)
            {
                foreach (int dia in dto.DiasRepeticion)
                {
                    if (dia < 0 || dia > 6)
                        throw new ArgumentException("DiasRepeticion debe contener valores entre 0 y 6 (0=Domingo, 6=Sábado).");
                }
            }
            else
            {
                if (!dto.FechaLimite.HasValue)
                    throw new ArgumentException("FechaLimite es obligatoria para tareas puntuales.", nameof(dto.FechaLimite));
            }

            var createPlantilla = _mapper.Map<CreatePlantillaTareaDto>(dto);
            createPlantilla.DiasRepeticion = dto.DiasRepeticion ?? new List<int>();
            createPlantilla.UsuariosAsignacion = dto.UsuariosAsignacion ?? new List<string>();

            var tareas = new List<Tarea>();

            if (esPuntual)
            {
                var tarea = _mapper.Map<Tarea>(dto);
                tarea.DiaSemana = -1;
                tarea.Estado = TareaEstado.Pendiente;

                if (dto.UsuariosAsignacion != null && dto.UsuariosAsignacion.Count > 0)
                {
                    if (dto.UsuariosAsignacion.Count != 1)
                        throw new ArgumentException($"Tarea puntual requiere exactamente 1 usuario en UsuariosAsignacion. Se recibieron {dto.UsuariosAsignacion.Count}.");
                    tarea.UsuarioEspacioId = dto.UsuariosAsignacion[0];
                }
                else
                {
                    tarea.UsuarioEspacioId = null;
                }

                tarea.FechaLimite = dto.FechaLimite;

                if (dto.HoraLimite != default(TimeOnly))
                    tarea.HoraLimite = dto.HoraLimite;

                createPlantilla.TareasId.Add(tarea.Id!);
                tareas.Add(tarea);
            }
            else
            {
                var users = dto.UsuariosAsignacion ?? new List<string>();
                var days = dto.DiasRepeticion ?? new List<int>();
                var taskCount = days.Count;

                if (users.Count > 0 && users.Count != 1 && users.Count != taskCount)
                    throw new ArgumentException($"Si envías usuarios, el número de usuarios ({users.Count}) debe ser 1 o coincidir con el número de tareas a crear ({taskCount}).");

                for (int i = 0; i < days.Count; i++)
                {
                    var dia = days[i];
                    var tarea = _mapper.Map<Tarea>(dto);
                    tarea.DiaSemana = dia;
                    tarea.Estado = TareaEstado.Pendiente;
                    tarea.FechaLimite = null;

                    if (users.Count == 0)
                        tarea.UsuarioEspacioId = null;
                    else
                        tarea.UsuarioEspacioId = users.Count == 1 ? users[0] : users[i];

                    if (dto.HoraLimite != default(TimeOnly))
                        tarea.HoraLimite = dto.HoraLimite;

                    createPlantilla.TareasId.Add(tarea.Id!);
                    tareas.Add(tarea);
                }
            }

            var plantillaId = await _ptservice.AddAsync(createPlantilla, espacioid);

            foreach (var tarea in tareas)
            {
                tarea.PlantillaId = plantillaId;
            }

            var ids = await _repository.AddAsyncList(tareas);
            return plantillaId;
        }

        private async Task ResetCompletedRepeatedTasksAsync(PlantillaTareaDto plantilla, CancellationToken ct = default)
        {
            if (plantilla.DiasRepeticion == null || plantilla.DiasRepeticion.Count == 0)
                return;

            if (!IsPlantillaActive(plantilla))
                return;

            try
            {
                foreach (var tareaId in plantilla.TareasId ?? new List<string>())
                {
                    var tarea = await _repository.GetAsync(plantilla.PlantillaId, tareaId, ct);
                    if (tarea == null) continue;

                    if (tarea.DiaSemana < 0 || tarea.Estado != TareaEstado.Completada)
                        continue;

                    tarea.Estado = TareaEstado.Pendiente;
                    tarea.FechaRealizacion = null;

                    await _repository.UpdateAsync(tareaId, tarea, merge: true, ct);
                    _logger.LogDebug("Tarea repetida {TareaId} reseteada a Pendiente.", tareaId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reseteando tareas completadas para plantilla {PlantillaId}", plantilla.PlantillaId);
            }
        }

        private static bool IsPlantillaActive(PlantillaTareaDto plantilla)
        {
            if (plantilla.DiasRepeticion == null || plantilla.DiasRepeticion.Count == 0)
                return true;

            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

            if (plantilla.StartDate.HasValue && hoy < plantilla.StartDate.Value)
                return false;

            if (plantilla.EndDate.HasValue && hoy > plantilla.EndDate.Value)
                return false;

            return true;
        }

        private async Task AwardKarmaToUserAsync(string usuarioEspacioId, int karma, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioEspacioId) || karma <= 0)
                return;

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

        public async Task<IEnumerable<TareaDto>> GetByDiaAndEstadoAsync(
            string espacioid,
            int diaSemana,
            TareaEstado estado)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));

            if (diaSemana < 0 || diaSemana > 6)
                throw new ArgumentException("Día debe estar entre 0 y 6.", nameof(diaSemana));

            var pttareas = await _ptservice.GetAllByEspacioAsync(espacioid);
            var tareasFiltered = new List<TareaDto>();

            foreach (var plantilla in pttareas)
            {
                await ResetCompletedRepeatedTasksAsync(plantilla);

                var pt = plantilla.Adapt<PlantillaTarea>();

                foreach (var tareaId in plantilla.TareasId ?? new List<string>())
                {
                    var tarea = await _repository.GetAsync(plantilla.PlantillaId, tareaId);
                    if (tarea == null) continue;

                    if (tarea.DiaSemana != diaSemana) continue;

                    if (!MatchesEstado(tarea, pt, estado)) continue;

                    var dto = _mapper.Map<TareaDto>(tarea);
                    dto.Nombre = plantilla.Nombre;
                    dto.Descripcion = plantilla.Descripcion;
                    dto.karma = plantilla.karma;
                    dto.HoraLimite = tarea.HoraLimite;
                    dto.FacturaId = plantilla.FacturaId;
                    dto.Estado = tarea.Estado.ToString();
                    dto.Overdue = IsOverdue(tarea, pt);
                    dto.EsPuntual = tarea.DiaSemana == -1;
                    dto.UsuarioEspacioId = tarea.UsuarioEspacioId;

                    tareasFiltered.Add(dto);
                }
            }

            return tareasFiltered;
        }

        public async Task<IEnumerable<TareaDto>> GetByEstadoAsync(
            string espacioid,
            TareaEstado estado)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));

            var pttareas = await _ptservice.GetAllByEspacioAsync(espacioid);
            var tareasFiltered = new List<TareaDto>();

            foreach (var plantilla in pttareas)
            {
                await ResetCompletedRepeatedTasksAsync(plantilla);

                var pt = plantilla.Adapt<PlantillaTarea>();

                foreach (var tareaId in plantilla.TareasId ?? new List<string>())
                {
                    var tarea = await _repository.GetAsync(plantilla.PlantillaId, tareaId);
                    if (tarea == null) continue;

                    if (!MatchesEstado(tarea, pt, estado)) continue;

                    var dto = _mapper.Map<TareaDto>(tarea);
                    dto.Nombre = plantilla.Nombre;
                    dto.Descripcion = plantilla.Descripcion;
                    dto.karma = plantilla.karma;
                    dto.HoraLimite = tarea.HoraLimite;
                    dto.FacturaId = plantilla.FacturaId;
                    dto.Estado = tarea.Estado.ToString();
                    dto.Overdue = IsOverdue(tarea, pt);
                    dto.EsPuntual = tarea.DiaSemana == -1;
                    dto.UsuarioEspacioId = tarea.UsuarioEspacioId;

                    tareasFiltered.Add(dto);
                }
            }

            return tareasFiltered;
        }

        public async Task<IEnumerable<TareaDto>> FilterAsync(string espacioid, int? diaSemana = null, string? estado = null, string? usuarioId = null)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));

            if (diaSemana.HasValue && (diaSemana < 0 || diaSemana > 6))
                throw new ArgumentException("diaSemana debe estar entre 0 y 6.");

            TareaEstado? parsedEstado = null;
            if (!string.IsNullOrWhiteSpace(estado))
            {
                if (!Enum.TryParse<TareaEstado>(estado, ignoreCase: true, out var p))
                    throw new ArgumentException("estado no válido. Valores válidos: Pendiente, FueraDePlazo, Completada, CompletadaFueradePlazo");
                parsedEstado = p;
            }

            return await FilterByMultipleCriteriaAsync(espacioid, diaSemana, parsedEstado, usuarioId);
        }

        private async Task<IEnumerable<TareaDto>> FilterByMultipleCriteriaAsync(
            string espacioid, 
            int? diaSemana, 
            TareaEstado? estado,
            string? usuarioId)
        {
            var pttareas = await _ptservice.GetAllByEspacioAsync(espacioid);
            var tareasFiltered = new List<TareaDto>();

            foreach (var plantilla in pttareas)
            {
                bool plantillaActive = IsPlantillaActive(plantilla);
                bool isRepeated = plantilla.DiasRepeticion != null && plantilla.DiasRepeticion.Count > 0;
                
                if (isRepeated && !plantillaActive)
                {
                    _logger.LogDebug("Plantilla {PlantillaId} ha expirado (EndDate pasado). Tareas repetidas excluidas del filtro.", plantilla.PlantillaId);
                    continue;
                }

                if (plantillaActive)
                    await ResetCompletedRepeatedTasksAsync(plantilla);

                var pt = plantilla.Adapt<PlantillaTarea>();

                foreach (var tareaId in plantilla.TareasId ?? new List<string>())
                {
                    var tarea = await _repository.GetAsync(plantilla.PlantillaId, tareaId);
                    if (tarea == null) continue;

                    if (diaSemana.HasValue && tarea.DiaSemana != diaSemana)
                        continue;

                    if (estado.HasValue && !MatchesEstado(tarea, pt, estado.Value))
                        continue;

                    if (!string.IsNullOrWhiteSpace(usuarioId) && tarea.UsuarioEspacioId != usuarioId)
                        continue;

                    var dto = _mapper.Map<TareaDto>(tarea);
                    dto.Nombre = plantilla.Nombre;
                    dto.Descripcion = plantilla.Descripcion;
                    dto.karma = plantilla.karma;
                    dto.HoraLimite = tarea.HoraLimite;
                    dto.FacturaId = plantilla.FacturaId;
                    dto.Estado = tarea.Estado.ToString();
                    dto.Overdue = IsOverdue(tarea, pt);
                    dto.EsPuntual = tarea.DiaSemana == -1;
                    dto.UsuarioEspacioId = tarea.UsuarioEspacioId;

                    tareasFiltered.Add(dto);
                }
            }

            return tareasFiltered;
        }

        public async Task<IEnumerable<PlantillaTareaDto>> GetAllByEspacioAsync(string espacioid)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));

            var pttareas = await _ptservice.GetAllByEspacioAsync(espacioid);
            if (pttareas.Any() == false)
                return Enumerable.Empty<PlantillaTareaDto>();
            return _mapper.Map<IEnumerable<PlantillaTareaDto>>(pttareas);
        }

        public async Task<PlantillaTareaDto> GetByEspacioAndIdAsync(string espacioid, string id)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            var pttarea = await _ptservice.GetByEspacioAndIdAsync(espacioid, id);
            if (pttarea == null)
                throw new ArgumentException("La plantilla no existe o no pertenece al espacio especificado.", nameof(id));
            return _mapper.Map<PlantillaTareaDto>(pttarea);
        }

        public async Task<TareaDto?> GetByEspacioAndPlantillaAndTareaAsync(string espacioid, string plantillaId, string tareaId)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(plantillaId))
                throw new ArgumentNullException(nameof(plantillaId));
            if (string.IsNullOrWhiteSpace(tareaId))
                throw new ArgumentNullException(nameof(tareaId));

            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, plantillaId);
            if (plantilla == null)
                return null;

            var tarea = await _repository.GetAsync(plantillaId, tareaId);
            if (tarea == null)
                return null;

            var dto = _mapper.Map<TareaDto>(tarea);
            dto.Nombre = plantilla.Nombre;
            dto.Descripcion = plantilla.Descripcion;
            dto.karma = plantilla.karma;
            dto.HoraLimite = tarea.HoraLimite;
            dto.FacturaId = plantilla.FacturaId;
            dto.Estado = tarea.Estado.ToString();
            dto.Overdue = IsOverdue(tarea, plantilla.Adapt<PlantillaTarea>());
            dto.EsPuntual = tarea.DiaSemana == -1;
            dto.UsuarioEspacioId = tarea.UsuarioEspacioId;

            return dto;
        }

        public async Task<TareaDto?> UpdateCompleteAsync(string espacioid, string plantillaid, string tareaid, UpdateTareaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(plantillaid))
                throw new ArgumentNullException(nameof(plantillaid));
            if (string.IsNullOrWhiteSpace(tareaid))
                throw new ArgumentNullException(nameof(tareaid));
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, plantillaid);
            if (plantilla == null)
                return null;

            var existing = await _repository.GetAsync(plantillaid, tareaid, ct);
            if (existing == null)
                return null;

            if (!string.IsNullOrWhiteSpace(dto.UsuarioEspacioId))
            {
                var userValidation = await _ptservice.ValidateUsuarioEspacioExistAsync(espacioid, dto.UsuarioEspacioId);
                if (!userValidation.IsValid)
                    throw new InvalidOperationException(userValidation.ErrorMessage);
            }

            var domPlantilla = plantilla.Adapt<PlantillaTarea>();

            if (!string.IsNullOrWhiteSpace(dto.Estado))
            {
                if (Enum.TryParse<TareaEstado>(dto.Estado, ignoreCase: true, out var parsedEstado))
                {
                    if (parsedEstado == TareaEstado.Pendiente)
                    {
                        var isOverdue = IsOverdueWithUpdates(existing, domPlantilla, dto);
                        if (isOverdue)
                            throw new InvalidOperationException("No se puede marcar como pendiente una tarea que está overdue.");
                    }
                    if (parsedEstado == TareaEstado.Completada && !dto.FechaRealizacion.HasValue)
                    {
                        dto.FechaRealizacion = DateTime.UtcNow;
                    }
                    
                    bool isOverdueNow = false;
                    if (parsedEstado == TareaEstado.Completada || parsedEstado == TareaEstado.CompletadaFueradePlazo)
                    {
                        isOverdueNow = IsOverdueWithUpdates(existing, domPlantilla, dto);
                        
                        if (isOverdueNow)
                        {
                            parsedEstado = TareaEstado.CompletadaFueradePlazo;
                        }
                    }
                    
                    if ((parsedEstado == TareaEstado.Completada || parsedEstado == TareaEstado.CompletadaFueradePlazo) 
                        && existing.Estado != TareaEstado.Completada 
                        && existing.Estado != TareaEstado.CompletadaFueradePlazo
                        && !string.IsNullOrWhiteSpace(existing.UsuarioEspacioId))
                    {
                        await AwardKarmaToUserAsync(existing.UsuarioEspacioId, plantilla.karma, ct);
                    }
                    
                    existing.Estado = parsedEstado;
                }
                else
                {
                    throw new ArgumentException($"Estado '{dto.Estado}' no válido. Valores: Pendiente, FueraDePlazo, Completada, CompletadaFueradePlazo");
                }
            }

            var domain = _mapper.Map<Tarea>(dto);
            domain.Id = tareaid;
            domain.PlantillaId = plantillaid;
            if (!string.IsNullOrWhiteSpace(dto.Estado) && Enum.TryParse<TareaEstado>(dto.Estado, ignoreCase: true, out var estado))
                domain.Estado = estado;

            if ((domain.Estado == TareaEstado.Completada || domain.Estado == TareaEstado.CompletadaFueradePlazo))
            {
                var dueUtc = GetDueUtcForTask(domain, domPlantilla);
                var nowUtc = DateTime.UtcNow;
                if (dueUtc.HasValue && nowUtc > dueUtc.Value)
                {
                    domain.Prorroga = nowUtc - dueUtc.Value;
                }
            }

            await _repository.UpdateAsync(tareaid, domain, merge: false, ct);

            var updated = await _repository.GetAsync(plantillaid, tareaid, ct);
            if (updated == null)
                return null;

            var dtoResp = _mapper.Map<TareaDto>(updated);
            dtoResp.Nombre = plantilla.Nombre!;
            dtoResp.Descripcion = plantilla.Descripcion;
            dtoResp.karma = plantilla.karma;
            dtoResp.HoraLimite = updated.HoraLimite;
            dtoResp.FacturaId = plantilla.FacturaId;
            dtoResp.Estado = updated.Estado.ToString();
            dtoResp.Overdue = IsOverdue(updated, domPlantilla);
            dtoResp.EsPuntual = updated.DiaSemana == -1;
            dtoResp.UsuarioEspacioId = updated.UsuarioEspacioId;

            return dtoResp;
        }

        public async Task<bool> DeleteAsync(string espacioid, string plantillaid, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(plantillaid))
                throw new ArgumentNullException(nameof(plantillaid));

            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, plantillaid);
            if (plantilla == null)
                return false;

            if (plantilla.TareasId == null || plantilla.TareasId.Count == 0)
            {
                var resultat = await _ptservice.DeleteAsync(espacioid, plantillaid);
                return resultat;
            }

            foreach (string tareaid in plantilla.TareasId)
            {
                if (string.IsNullOrWhiteSpace(tareaid))
                    continue;

                try
                {
                    await _repository.DeleteAsync(tareaid, ct);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error eliminando tarea {tareaid} de plantilla {plantillaid}.", ex);
                }
            }

            var result = await _ptservice.DeleteAsync(espacioid, plantillaid);
            if (!result)
                throw new InvalidOperationException($"No se pudo eliminar la plantilla {plantillaid}.");

            return true;
        }

        public async Task<TareaDto?> UpdateMergeAsync(string espacioid, string plantillaid, string tareaid, UpdateTareaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(plantillaid))
                throw new ArgumentNullException(nameof(plantillaid));
            if (string.IsNullOrWhiteSpace(tareaid))
                throw new ArgumentNullException(nameof(tareaid));
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, plantillaid);
            if (plantilla == null)
                return null;

            var existing = await _repository.GetAsync(plantillaid, tareaid, ct);
            if (existing == null)
                return null;

            if (!string.IsNullOrWhiteSpace(dto.UsuarioEspacioId))
            {
                var userValidation = await _ptservice.ValidateUsuarioEspacioExistAsync(espacioid, dto.UsuarioEspacioId);
                if (!userValidation.IsValid)
                    throw new InvalidOperationException(userValidation.ErrorMessage);
            }

            var domPlantilla = plantilla.Adapt<PlantillaTarea>();

            if (!string.IsNullOrWhiteSpace(dto.Estado))
            {
                if (Enum.TryParse<TareaEstado>(dto.Estado, ignoreCase: true, out var parsedEstado))
                {
                    if (parsedEstado == TareaEstado.Pendiente)
                    {
                        var isOverdue = IsOverdueWithUpdates(existing, domPlantilla, dto);
                        if (isOverdue)
                            throw new InvalidOperationException("No se puede marcar como pendiente una tarea que está overdue.");
                    }
                    if (parsedEstado == TareaEstado.Completada && !dto.FechaRealizacion.HasValue)
                    {
                        dto.FechaRealizacion = DateTime.UtcNow;
                    }
                    
                    bool isOverdueNow = false;
                    if (parsedEstado == TareaEstado.Completada || parsedEstado == TareaEstado.CompletadaFueradePlazo)
                    {
                        isOverdueNow = IsOverdueWithUpdates(existing, domPlantilla, dto);
                        
                        if (isOverdueNow)
                        {
                            parsedEstado = TareaEstado.CompletadaFueradePlazo;
                        }
                    }
                    
                    if ((parsedEstado == TareaEstado.Completada || parsedEstado == TareaEstado.CompletadaFueradePlazo) 
                        && existing.Estado != TareaEstado.Completada 
                        && existing.Estado != TareaEstado.CompletadaFueradePlazo
                        && !string.IsNullOrWhiteSpace(existing.UsuarioEspacioId))
                    {
                        await AwardKarmaToUserAsync(existing.UsuarioEspacioId, plantilla.karma, ct);
                    }
                    
                    existing.Estado = parsedEstado;
                }
                else
                {
                    throw new ArgumentException($"Estado '{dto.Estado}' no válido. Valores: Pendiente, FueraDePlazo, Completada, CompletadaFueradePlazo");
                }
            }

            _mapper.Map(dto, existing);
            if (!string.IsNullOrWhiteSpace(dto.Estado) && Enum.TryParse<TareaEstado>(dto.Estado, ignoreCase: true, out var estado2))
                existing.Estado = estado2;

            if ((existing.Estado == TareaEstado.Completada || existing.Estado == TareaEstado.CompletadaFueradePlazo))
            {
                var dueUtc = GetDueUtcForTask(existing, domPlantilla);
                var nowUtc = DateTime.UtcNow;
                if (dueUtc.HasValue && nowUtc > dueUtc.Value)
                {
                    existing.Prorroga = nowUtc - dueUtc.Value;
                }
            }

            await _repository.UpdateAsync(tareaid, existing, merge: true, ct);

            var updated = await _repository.GetAsync(plantillaid, tareaid, ct);
            if (updated == null)
                return null;

            var dtoResp = _mapper.Map<TareaDto>(updated);
            dtoResp.Nombre = plantilla.Nombre!;
            dtoResp.Descripcion = plantilla.Descripcion;
            dtoResp.karma = plantilla.karma;
            dtoResp.HoraLimite = updated.HoraLimite;
            dtoResp.FacturaId = plantilla.FacturaId;
            dtoResp.Estado = updated.Estado.ToString();
            dtoResp.Overdue = IsOverdue(updated, domPlantilla);
            dtoResp.EsPuntual = updated.DiaSemana == -1;
            dtoResp.UsuarioEspacioId = updated.UsuarioEspacioId;

            return dtoResp;
        }

        public async Task<TareaDto?> UpdatePartialAsync(string espacioid, string plantillaid, string tareaid, UpdateTareaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(plantillaid))
                throw new ArgumentNullException(nameof(plantillaid));
            if (string.IsNullOrWhiteSpace(tareaid))
                throw new ArgumentNullException(nameof(tareaid));
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, plantillaid);
            if (plantilla == null)
                return null;

            var existing = await _repository.GetAsync(plantillaid, tareaid, ct);
            if (existing == null)
                return null;

            if (!string.IsNullOrWhiteSpace(dto.UsuarioEspacioId))
            {
                var userValidation = await _ptservice.ValidateUsuarioEspacioExistAsync(espacioid, dto.UsuarioEspacioId);
                if (!userValidation.IsValid)
                    throw new InvalidOperationException(userValidation.ErrorMessage);
            }

            var domPlantilla = plantilla.Adapt<PlantillaTarea>();

            if (!string.IsNullOrWhiteSpace(dto.Estado))
            {
                if (Enum.TryParse<TareaEstado>(dto.Estado, ignoreCase: true, out var parsedEstado))
                {
                    if (parsedEstado == TareaEstado.Pendiente)
                    {
                        var isOverdue = IsOverdueWithUpdates(existing, domPlantilla, dto);
                        if (isOverdue)
                            throw new InvalidOperationException("No se puede marcar como pendiente una tarea que está overdue.");
                    }
                    if (parsedEstado == TareaEstado.Completada && !dto.FechaRealizacion.HasValue)
                    {
                        dto.FechaRealizacion = DateTime.UtcNow;
                    }
                    
                    bool isOverdueNow = false;
                    if (parsedEstado == TareaEstado.Completada || parsedEstado == TareaEstado.CompletadaFueradePlazo)
                    {
                        isOverdueNow = IsOverdueWithUpdates(existing, domPlantilla, dto);
                        
                        if (isOverdueNow)
                        {
                            parsedEstado = TareaEstado.CompletadaFueradePlazo;
                        }
                    }
                    
                    if ((parsedEstado == TareaEstado.Completada || parsedEstado == TareaEstado.CompletadaFueradePlazo) 
                        && existing.Estado != TareaEstado.Completada 
                        && existing.Estado != TareaEstado.CompletadaFueradePlazo
                        && !string.IsNullOrWhiteSpace(existing.UsuarioEspacioId))
                    {
                        await AwardKarmaToUserAsync(existing.UsuarioEspacioId, plantilla.karma, ct);
                    }
                }
                else
                {
                    throw new ArgumentException($"Estado '{dto.Estado}' no válido. Valores: Pendiente, FueraDePlazo, Completada, CompletadaFueradePlazo");
                }
            }

            var updates = ObtenerActualizacionesDesdeDto(dto);
            if (updates.Count == 0)
            {
                var current = existing;
                var dtoResp = _mapper.Map<TareaDto>(current);
                dtoResp.Nombre = plantilla.Nombre!;
                dtoResp.Descripcion = plantilla.Descripcion;
                dtoResp.karma = plantilla.karma;
                dtoResp.HoraLimite = current.HoraLimite;
                dtoResp.FacturaId = plantilla.FacturaId;
                dtoResp.Estado = current.Estado.ToString();
                dtoResp.Overdue = IsOverdue(current, domPlantilla);
                dtoResp.EsPuntual = current.DiaSemana == -1;
                dtoResp.UsuarioEspacioId = current.UsuarioEspacioId;
                return dtoResp;
            }

            await _repository.UpdateAsync(tareaid, updates, useSetMerge: false, ct);

            if (updates.ContainsKey("Estado") && (updates["Estado"]?.ToString() == TareaEstado.Completada.ToString() || updates["Estado"]?.ToString() == TareaEstado.CompletadaFueradePlazo.ToString()))
            {
                var updatedAfter = await _repository.GetAsync(plantillaid, tareaid, ct);
                if (updatedAfter != null)
                {
                    var dueUtc = GetDueUtcForTask(updatedAfter, domPlantilla);
                    var nowUtc = DateTime.UtcNow;
                    if (dueUtc.HasValue && nowUtc > dueUtc.Value)
                    {
                        updatedAfter.Prorroga = nowUtc - dueUtc.Value;
                        await _repository.UpdateAsync(tareaid, updatedAfter, merge: true, ct);
                    }
                }
            }

            var updated = await _repository.GetAsync(plantillaid, tareaid, ct);
            if (updated == null)
                return null;

            var dtoResult = _mapper.Map<TareaDto>(updated);
            dtoResult.Nombre = plantilla.Nombre!;
            dtoResult.Descripcion = plantilla.Descripcion;
            dtoResult.karma = plantilla.karma;
            dtoResult.HoraLimite = updated.HoraLimite;
            dtoResult.FacturaId = plantilla.FacturaId;
            dtoResult.Estado = updated.Estado.ToString();
            dtoResult.Overdue = IsOverdue(updated, domPlantilla);
            dtoResult.EsPuntual = updated.DiaSemana == -1;
            dtoResult.UsuarioEspacioId = updated.UsuarioEspacioId;

            return dtoResult;
        }

        public async Task<TareaDto> UpdateAsync(string espacioid, string plantillaid, string tareaid, UpdateTareaDto dto)
        {
            var res = await UpdatePartialAsync(espacioid, plantillaid, tareaid, dto);
            return res!;
        }

        private IDictionary<string, object> ObtenerActualizacionesDesdeDto(UpdateTareaDto dto)
        {
            var updates = new Dictionary<string, object>();
            if (dto.FechaRealizacion.HasValue)
                updates["FechaRealizacion"] = dto.FechaRealizacion.Value;
            if (dto.Foto != null)
                updates["Foto"] = dto.Foto;
            if (dto.Prorroga.HasValue)
                updates["ProrrogaSegundos"] = dto.Prorroga.Value.TotalSeconds;

            if (dto.HoraLimite.HasValue)
                updates["HoraLimite"] = dto.HoraLimite.Value.ToString("HH:mm");

            if (!string.IsNullOrWhiteSpace(dto.Estado))
            {
                if (Enum.TryParse<TareaEstado>(dto.Estado, ignoreCase: true, out var parsed))
                    updates["Estado"] = parsed.ToString();
            }

            if (!string.IsNullOrWhiteSpace(dto.UsuarioEspacioId))
                updates["UsuarioEspacioId"] = dto.UsuarioEspacioId;
            return updates;
        }

        private static bool IsOverdue(Tarea tarea, PlantillaTarea plantilla)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(plantilla.TimeZoneId);
            var nowUtc = DateTime.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);

            if (tarea.FechaRealizacion.HasValue)
                return false;

            if (tarea.DiaSemana == -1)
            {
                if (!tarea.FechaLimite.HasValue)
                    return false;

                if (!tarea.HoraLimite.HasValue)
                    return false;

                var fecha = tarea.FechaLimite.Value.Date;
                var horaLimite = tarea.HoraLimite.Value;

                var dueLocal = new DateTime(fecha.Year, fecha.Month, fecha.Day,
                                            horaLimite.Hour, horaLimite.Minute, 0, DateTimeKind.Unspecified);
                var dueUtc = new DateTimeOffset(dueLocal, tz.GetUtcOffset(dueLocal)).UtcDateTime;

                if (plantilla.GracePeriodMinutes.HasValue)
                    dueUtc = dueUtc.AddMinutes(plantilla.GracePeriodMinutes.Value);

                return nowUtc >= dueUtc;
            }

            var hoy = DateOnly.FromDateTime(nowLocal.Date);
            if (plantilla.EndDate.HasValue && hoy > plantilla.EndDate.Value)
                return false;

            if (tarea.DiaSemana != (int)nowLocal.DayOfWeek)
                return false;

            if (!tarea.HoraLimite.HasValue)
                return false;

            var occurrenceDate = nowLocal.Date;
            var horaLimiteRep = tarea.HoraLimite.Value;

            var dueLocalRep = new DateTime(occurrenceDate.Year, occurrenceDate.Month, occurrenceDate.Day,
                                        horaLimiteRep.Hour, horaLimiteRep.Minute, 0, DateTimeKind.Unspecified);
            var dueUtcRep = new DateTimeOffset(dueLocalRep, tz.GetUtcOffset(dueLocalRep)).UtcDateTime;

            if (plantilla.GracePeriodMinutes.HasValue)
                dueUtcRep = dueUtcRep.AddMinutes(plantilla.GracePeriodMinutes.Value);

            return nowUtc >= dueUtcRep;
        }

        private static DateTime? GetDueUtcForTask(Tarea tarea, PlantillaTarea plantilla)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(plantilla.TimeZoneId);
            
            if (tarea.DiaSemana == -1)
            {
                if (!tarea.FechaLimite.HasValue)
                    return null;

                if (!tarea.HoraLimite.HasValue)
                    return null;

                var fecha = tarea.FechaLimite.Value.Date;
                var horaLimite = tarea.HoraLimite.Value;

                var dueLocal = new DateTime(fecha.Year, fecha.Month, fecha.Day,
                                            horaLimite.Hour, horaLimite.Minute, 0, DateTimeKind.Unspecified);
                var dueUtc = new DateTimeOffset(dueLocal, tz.GetUtcOffset(dueLocal)).UtcDateTime;

                if (plantilla.GracePeriodMinutes.HasValue)
                    dueUtc = dueUtc.AddMinutes(plantilla.GracePeriodMinutes.Value);

                return dueUtc;
            }

            var nowUtc = DateTime.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);

            var hoy = DateOnly.FromDateTime(nowLocal.Date);
            if (plantilla.EndDate.HasValue && hoy > plantilla.EndDate.Value)
                return null;

            int targetDay = tarea.DiaSemana;
            var daysDiff = ((int)nowLocal.DayOfWeek - targetDay + 7) % 7;
            var occurrenceDate = nowLocal.Date.AddDays(-daysDiff);

            if (!tarea.HoraLimite.HasValue)
                return null;

            var horaLimiteRep = tarea.HoraLimite.Value;
            var dueLocalRep = new DateTime(occurrenceDate.Year, occurrenceDate.Month, occurrenceDate.Day,
                                        horaLimiteRep.Hour, horaLimiteRep.Minute, 0, DateTimeKind.Unspecified);
            var dueUtcRep = new DateTimeOffset(dueLocalRep, tz.GetUtcOffset(dueLocalRep)).UtcDateTime;

            if (plantilla.GracePeriodMinutes.HasValue)
                dueUtcRep = dueUtcRep.AddMinutes(plantilla.GracePeriodMinutes.Value);

            return dueUtcRep;
        }

        private static bool IsOverdueWithUpdates(Tarea existing, PlantillaTarea plantilla, UpdateTareaDto? dto)
        {
            if (dto == null)
                return IsOverdue(existing, plantilla);

            var temp = existing.Adapt<Tarea>();

            if (dto.Prorroga.HasValue)
                temp.Prorroga = dto.Prorroga.Value;
            if (dto.FechaRealizacion.HasValue)
                temp.FechaRealizacion = dto.FechaRealizacion.Value;

            if (!string.IsNullOrWhiteSpace(dto.Estado) && Enum.TryParse<TareaEstado>(dto.Estado, ignoreCase: true, out var parsed) && parsed == TareaEstado.Completada)
            {
                if (dto.FechaRealizacion.HasValue)
                    return false;
            }

            return IsOverdue(temp, plantilla);
        }

        private bool MatchesEstado(Tarea tarea, PlantillaTarea plantilla, TareaEstado estado)
        {
            return estado switch
            {
                TareaEstado.Completada => tarea.Estado == TareaEstado.Completada || tarea.Estado == TareaEstado.CompletadaFueradePlazo,
                TareaEstado.CompletadaFueradePlazo => tarea.Estado == TareaEstado.CompletadaFueradePlazo,
                TareaEstado.Pendiente => tarea.Estado == TareaEstado.Pendiente && !IsOverdue(tarea, plantilla),
                TareaEstado.FueraDePlazo => tarea.Estado == TareaEstado.FueraDePlazo || (tarea.Estado != TareaEstado.Completada && tarea.Estado != TareaEstado.CompletadaFueradePlazo && IsOverdue(tarea, plantilla)),
                _ => false
            };
        }
    }
}