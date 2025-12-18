using Convivia.Domain.Entities;
using Convivia.Domain.Repositories;
using Convivia.Shared.DTOs;
using Mapster;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Application.Services
{
    /// <summary>
    /// Estados posibles de una tarea
    /// </summary>
    public enum TareaEstado
    {
        Pendiente,     // No completada, dentro de plazo
        FueraDePlazo,  // No completada, pasó hora límite (overdue)
        Completada     // Completada
    }

    public class TareaService
    {
        private readonly ITareaRepository _repository;
        private readonly IMapper _mapper;
        private readonly PlantillaTareaService _ptservice;

        private static readonly int[] KarmasValidos = { 5, 15, 25, 50 };

        public TareaService(ITareaRepository tarea, IMapper _mapper, PlantillaTareaService ptservice)
        {
            _repository = tarea ?? throw new ArgumentNullException(nameof(tarea));
            this._mapper = _mapper ?? throw new ArgumentNullException(nameof(_mapper));
            _ptservice = ptservice ?? throw new ArgumentNullException(nameof(ptservice));
        }

        public async Task<string> AddAsync(string espacioid, CreateTareaDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));

            // Validación 1: Verificar que el Espacio existe
            var plantillaValidation = await _ptservice.ValidateEspacioExistsAsync(espacioid);
            if (!plantillaValidation.IsValid)
                throw new InvalidOperationException(plantillaValidation.ErrorMessage);

            // Validación 2: Verificar karma válido
            if (!KarmasValidos.Contains(dto.karma))
                throw new ArgumentException("karma debe ser 5, 15, 25 o 50.", nameof(dto.karma));

            // Validación 3: Validar tipo de tarea
            bool esPuntual = dto.DiasRepeticion == null || dto.DiasRepeticion.Count == 0;

            if (!esPuntual)
            {
                // Tarea repetida: validar días
                foreach (int dia in dto.DiasRepeticion)
                {
                    if (dia < 0 || dia > 6)
                        throw new ArgumentException("DiasRepeticion debe contener valores entre 0 y 6 (0=Domingo, 6=Sábado).");
                }

                // Para tarea repetida: FechaFin opcional, FechaLimite opcional
            }
            else
            {
                // Tarea puntual: FechaLimite obligatoria, sin días de repetición
                if (!dto.FechaLimite.HasValue)
                    throw new ArgumentException("FechaLimite es obligatoria para tareas puntuales.", nameof(dto.FechaLimite));
            }

            // Validación 4: Si es puntual, requiere usuario
            if (esPuntual && string.IsNullOrWhiteSpace(dto.UsuarioEspacioId))
                throw new ArgumentException("Una tarea puntual requiere UsuarioEspacioId.", nameof(dto.UsuarioEspacioId));

            // Validación 5: Verificar usuario si está presente
            if (!string.IsNullOrWhiteSpace(dto.UsuarioEspacioId))
            {
                var userValidation = await _ptservice.ValidateUsuarioEspacioExistAsync(espacioid, dto.UsuarioEspacioId);
                if (!userValidation.IsValid)
                    throw new InvalidOperationException(userValidation.ErrorMessage);
            }

            var createPlantilla = _mapper.Map<CreatePlantillaTareaDto>(dto);
            var tareas = new List<Tarea>();

            if (esPuntual)
            {
                // Tarea puntual: 1 sola tarea
                var tarea = _mapper.Map<Tarea>(dto);
                tarea.DiaSemana = -1; // Indicador de tarea puntual
                tarea.Completada = false;
                tarea.Disponible = true;
                tarea.UsuarioEspacioId = dto.UsuarioEspacioId;
                tarea.FechaLimite = dto.FechaLimite;
                createPlantilla.TareasId.Add(tarea.Id!);
                tareas.Add(tarea);
            }
            else
            {
                // Tarea repetida: 1 tarea por día de repetición
                foreach (int dia in createPlantilla.DiasRepeticion)
                {
                    var tarea = _mapper.Map<Tarea>(dto);
                    tarea.DiaSemana = dia;
                    tarea.Completada = false;
                    tarea.Disponible = true;
                    tarea.FechaLimite = dto.FechaLimite;
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

        /// <summary>
        /// Obtener tareas por día de la semana y estado.
        /// </summary>
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
                var pt = plantilla.Adapt<PlantillaTarea>();
                
                foreach (var tareaId in plantilla.TareasId ?? new List<string>())
                {
                    var tarea = await _repository.GetAsync(plantilla.PlantillaId, tareaId);
                    if (tarea == null) continue;

                    // Filtrar por día
                    if (tarea.DiaSemana != diaSemana) continue;

                    // Filtrar por estado
                    if (!MatchesEstado(tarea, pt, estado)) continue;

                    var dto = _mapper.Map<TareaDto>(tarea);
                    dto.Nombre = plantilla.Nombre;
                    dto.Descripcion = plantilla.Descripcion;
                    dto.karma = plantilla.karma;
                    dto.HoraLimite = plantilla.HoraLimite;
                    dto.FacturaId = plantilla.FacturaId;
                    dto.Overdue = IsOverdue(tarea, pt);
                    
                    if (dto.Overdue)
                        dto.Disponible = false;

                    tareasFiltered.Add(dto);
                }
            }

            return tareasFiltered;
        }

        /// <summary>
        /// Obtener tareas solo por estado.
        /// </summary>
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
                    dto.HoraLimite = plantilla.HoraLimite;
                    dto.FacturaId = plantilla.FacturaId;
                    dto.Overdue = IsOverdue(tarea, pt);
                    
                    if (dto.Overdue)
                        dto.Disponible = false;

                    tareasFiltered.Add(dto);
                }
            }

            return tareasFiltered;
        }

        /// <summary>
        /// Filtrar tareas por día y/o estado. Validaciones centralizadas aquí.
        /// </summary>
        public async Task<IEnumerable<TareaDto>> FilterAsync(string espacioid, int? diaSemana, string? estado)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));

            if (!diaSemana.HasValue && string.IsNullOrWhiteSpace(estado))
                throw new ArgumentException("Se requiere al menos 'diaSemana' o 'estado'.");

            if (diaSemana.HasValue && (diaSemana < 0 || diaSemana > 6))
                throw new ArgumentException("diaSemana debe estar entre 0 y 6.");

            TareaEstado? parsedEstado = null;
            if (!string.IsNullOrWhiteSpace(estado))
            {
                if (!Enum.TryParse<TareaEstado>(estado, ignoreCase: true, out var p))
                    throw new ArgumentException("estado no válido. Valores válidos: Pendiente, FueraDePlazo, Completada");
                parsedEstado = p;
            }

            // Ambos parámetros
            if (diaSemana.HasValue && parsedEstado.HasValue)
            {
                return await GetByDiaAndEstadoAsync(espacioid, diaSemana.Value, parsedEstado.Value);
            }

            // Solo día -> mantener comportamiento anterior (por defecto Pendiente)
            if (diaSemana.HasValue)
            {
                return await GetByDiaAndEstadoAsync(espacioid, diaSemana.Value, TareaEstado.Pendiente);
            }

            // Solo estado
            return await GetByEstadoAsync(espacioid, parsedEstado!.Value);
        }

        public async Task<IEnumerable<PlantillaTareaDto>> GetAllByEspacioAsync(string espacioid)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));

            var pttareas = await _ptservice.GetAllByEspacioAsync(espacioid);
            if (pttareas.Any() == false)
            {
                return Enumerable.Empty<PlantillaTareaDto>();
            }
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
            dto.HoraLimite = plantilla.HoraLimite;
            dto.FacturaId = plantilla.FacturaId;

            dto.Overdue = IsOverdue(tarea, plantilla.Adapt<PlantillaTarea>());

            if (dto.Overdue)
                dto.Disponible = false;

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

            // Validación: usuario si se actualiza
            if (!string.IsNullOrWhiteSpace(dto.UsuarioEspacioId))
            {
                var userValidation = await _ptservice.ValidateUsuarioEspacioExistAsync(espacioid, dto.UsuarioEspacioId);
                if (!userValidation.IsValid)
                    throw new InvalidOperationException(userValidation.ErrorMessage);
            }

            var domPlantilla = plantilla.Adapt<PlantillaTarea>();
            if (dto.Disponible == true)
            {
                var isOverdue = IsOverdue(existing, domPlantilla);
                if (isOverdue)
                    throw new InvalidOperationException("No se puede marcar como disponible una tarea que está overdue.");
            }

            var domain = _mapper.Map<Tarea>(dto);
            domain.Id = tareaid;
            domain.PlantillaId = plantillaid;

            await _repository.UpdateAsync(tareaid, domain, merge: false, ct);

            var updated = await _repository.GetAsync(plantillaid, tareaid, ct);
            if (updated == null)
                return null;

            var dtoResp = _mapper.Map<TareaDto>(updated);
            dtoResp.Nombre = plantilla.Nombre!;
            dtoResp.Descripcion = plantilla.Descripcion;
            dtoResp.karma = plantilla.karma;
            dtoResp.HoraLimite = plantilla.HoraLimite;
            dtoResp.FacturaId = plantilla.FacturaId;

            dtoResp.Overdue = IsOverdue(updated, domPlantilla);
            if (dtoResp.Overdue)
                dtoResp.Disponible = false;

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

            // Validación: usuario
            if (!string.IsNullOrWhiteSpace(dto.UsuarioEspacioId))
            {
                var userValidation = await _ptservice.ValidateUsuarioEspacioExistAsync(espacioid, dto.UsuarioEspacioId);
                if (!userValidation.IsValid)
                    throw new InvalidOperationException(userValidation.ErrorMessage);
            }

            var domPlantilla = plantilla.Adapt<PlantillaTarea>();
            if (dto.Disponible == true)
            {
                var isOverdue = IsOverdue(existing, domPlantilla);
                if (isOverdue)
                    throw new InvalidOperationException("No se puede marcar como disponible una tarea que está overdue.");
            }

            _mapper.Map(dto, existing);

            await _repository.UpdateAsync(tareaid, existing, merge: true, ct);

            var updated = await _repository.GetAsync(plantillaid, tareaid, ct);
            if (updated == null)
                return null;

            var dtoResp = _mapper.Map<TareaDto>(updated);
            dtoResp.Nombre = plantilla.Nombre!;
            dtoResp.Descripcion = plantilla.Descripcion;
            dtoResp.karma = plantilla.karma;
            dtoResp.HoraLimite = plantilla.HoraLimite;
            dtoResp.FacturaId = plantilla.FacturaId;

            dtoResp.Overdue = IsOverdue(updated, domPlantilla);
            if (dtoResp.Overdue)
                dtoResp.Disponible = false;

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

            // Validación: usuario
            if (!string.IsNullOrWhiteSpace(dto.UsuarioEspacioId))
            {
                var userValidation = await _ptservice.ValidateUsuarioEspacioExistAsync(espacioid, dto.UsuarioEspacioId);
                if (!userValidation.IsValid)
                    throw new InvalidOperationException(userValidation.ErrorMessage);
            }

            var domPlantilla = plantilla.Adapt<PlantillaTarea>();
            if (dto.Disponible == true)
            {
                var isOverdue = IsOverdue(existing, domPlantilla);
                if (isOverdue)
                    throw new InvalidOperationException("No se puede marcar como disponible una tarea que está overdue.");
            }

            var updates = ObtenerActualizacionesDesdeDto(dto);
            if (updates.Count == 0)
            {
                var current = existing;
                var dtoResp = _mapper.Map<TareaDto>(current);
                dtoResp.Nombre = plantilla.Nombre!;
                dtoResp.Descripcion = plantilla.Descripcion;
                dtoResp.karma = plantilla.karma;
                dtoResp.HoraLimite = plantilla.HoraLimite;
                dtoResp.FacturaId = plantilla.FacturaId;
                dtoResp.Overdue = IsOverdue(current, domPlantilla);
                if (dtoResp.Overdue)
                    dtoResp.Disponible = false;
                return dtoResp;
            }

            await _repository.UpdateAsync(tareaid, updates, useSetMerge: false, ct);

            var updated = await _repository.GetAsync(plantillaid, tareaid, ct);
            if (updated == null)
                return null;

            var dtoResult = _mapper.Map<TareaDto>(updated);
            dtoResult.Nombre = plantilla.Nombre!;
            dtoResult.Descripcion = plantilla.Descripcion;
            dtoResult.karma = plantilla.karma;
            dtoResult.HoraLimite = plantilla.HoraLimite;
            dtoResult.FacturaId = plantilla.FacturaId;
            dtoResult.Overdue = IsOverdue(updated, domPlantilla);
            if (dtoResult.Overdue)
                dtoResult.Disponible = false;

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
                updates["Prorroga"] = dto.Prorroga.Value;
            if (dto.Disponible.HasValue)
                updates["Disponible"] = dto.Disponible.Value;
            if (dto.Completada.HasValue)
                updates["Completada"] = dto.Completada.Value;
            if (!string.IsNullOrWhiteSpace(dto.UsuarioEspacioId))
                updates["UsuarioEspacioId"] = dto.UsuarioEspacioId;
            return updates;
        }

        private static bool IsOverdue(Tarea tarea, PlantillaTarea plantilla)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(plantilla.TimeZoneId);
            var nowUtc = DateTime.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);

            if (tarea.DiaSemana != (int)nowLocal.DayOfWeek)
                return false;

            var occurrenceDate = nowLocal.Date;
            var horaLimite = plantilla.HoraLimite;

            var dueLocal = new DateTime(occurrenceDate.Year, occurrenceDate.Month, occurrenceDate.Day,
                                        horaLimite.Hour, horaLimite.Minute, 0, DateTimeKind.Unspecified);
            var dueUtc = new DateTimeOffset(dueLocal, tz.GetUtcOffset(dueLocal)).UtcDateTime;

            if (plantilla.GracePeriodMinutes.HasValue)
                dueUtc = dueUtc.AddMinutes(plantilla.GracePeriodMinutes.Value);

            return nowUtc >= dueUtc;
        }

        /// <summary>
        /// Verifica si una tarea coincide con el estado especificado.
        /// </summary>
        private bool MatchesEstado(Tarea tarea, PlantillaTarea plantilla, TareaEstado estado)
        {
            return estado switch
            {
                TareaEstado.Completada => tarea.Completada,
                TareaEstado.Pendiente => !tarea.Completada && !IsOverdue(tarea, plantilla),
                TareaEstado.FueraDePlazo => !tarea.Completada && IsOverdue(tarea, plantilla),
                _ => false
            };
        }
    }
}