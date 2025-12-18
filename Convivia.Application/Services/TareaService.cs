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
            }
            else
            {
                // Tarea puntual: FechaLimite obligatoria
                if (!dto.FechaLimite.HasValue)
                    throw new ArgumentException("FechaLimite es obligatoria para tareas puntuales.", nameof(dto.FechaLimite));
            }

            // Validación 4: Verificar usuario si está presente (OPCIONAL)
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
                tarea.Estado = TareaEstado.Pendiente;
                tarea.UsuarioEspacioId = dto.UsuarioEspacioId; // opcional
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
                    tarea.Estado = TareaEstado.Pendiente;
                    tarea.FechaLimite = dto.FechaLimite;
                    tarea.UsuarioEspacioId = dto.UsuarioEspacioId; // opcional
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
                    dto.Estado = tarea.Estado.ToString();
                    dto.Overdue = IsOverdue(tarea, pt);

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
                    dto.Estado = tarea.Estado.ToString();
                    dto.Overdue = IsOverdue(tarea, pt);

                    tareasFiltered.Add(dto);
                }
            }

            return tareasFiltered;
        }

        /// <summary>
        /// Filtrar tareas por día y/o estado.
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

            if (diaSemana.HasValue && parsedEstado.HasValue)
                return await GetByDiaAndEstadoAsync(espacioid, diaSemana.Value, parsedEstado.Value);

            if (diaSemana.HasValue)
                return await GetByDiaAndEstadoAsync(espacioid, diaSemana.Value, TareaEstado.Pendiente);

            return await GetByEstadoAsync(espacioid, parsedEstado!.Value);
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
            dto.HoraLimite = plantilla.HoraLimite;
            dto.FacturaId = plantilla.FacturaId;
            dto.Estado = tarea.Estado.ToString();
            dto.Overdue = IsOverdue(tarea, plantilla.Adapt<PlantillaTarea>());

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

            // Validación: usuario si se actualiza (OPCIONAL)
            if (!string.IsNullOrWhiteSpace(dto.UsuarioEspacioId))
            {
                var userValidation = await _ptservice.ValidateUsuarioEspacioExistAsync(espacioid, dto.UsuarioEspacioId);
                if (!userValidation.IsValid)
                    throw new InvalidOperationException(userValidation.ErrorMessage);
            }

            var domPlantilla = plantilla.Adapt<PlantillaTarea>();

            // Parse estado si viene (string -> enum)
            if (!string.IsNullOrWhiteSpace(dto.Estado))
            {
                if (Enum.TryParse<TareaEstado>(dto.Estado, ignoreCase: true, out var parsedEstado))
                {
                    if (parsedEstado == TareaEstado.Pendiente)
                    {
                        var isOverdue = IsOverdue(existing, domPlantilla);
                        if (isOverdue)
                            throw new InvalidOperationException("No se puede marcar como pendiente una tarea que está overdue.");
                    }
                    existing.Estado = parsedEstado;
                }
                else
                {
                    throw new ArgumentException($"Estado '{dto.Estado}' no válido. Valores: Pendiente, FueraDePlazo, Completada");
                }
            }

            var domain = _mapper.Map<Tarea>(dto);
            domain.Id = tareaid;
            domain.PlantillaId = plantillaid;
            if (!string.IsNullOrWhiteSpace(dto.Estado) && Enum.TryParse<TareaEstado>(dto.Estado, ignoreCase: true, out var estado))
                domain.Estado = estado;

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
            dtoResp.Estado = updated.Estado.ToString();
            dtoResp.Overdue = IsOverdue(updated, domPlantilla);

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

            // Validación: usuario (OPCIONAL)
            if (!string.IsNullOrWhiteSpace(dto.UsuarioEspacioId))
            {
                var userValidation = await _ptservice.ValidateUsuarioEspacioExistAsync(espacioid, dto.UsuarioEspacioId);
                if (!userValidation.IsValid)
                    throw new InvalidOperationException(userValidation.ErrorMessage);
            }

            var domPlantilla = plantilla.Adapt<PlantillaTarea>();

            // Parse estado si viene (string -> enum)
            if (!string.IsNullOrWhiteSpace(dto.Estado))
            {
                if (Enum.TryParse<TareaEstado>(dto.Estado, ignoreCase: true, out var parsedEstado))
                {
                    if (parsedEstado == TareaEstado.Pendiente)
                    {
                        var isOverdue = IsOverdue(existing, domPlantilla);
                        if (isOverdue)
                            throw new InvalidOperationException("No se puede marcar como pendiente una tarea que está overdue.");
                    }
                    existing.Estado = parsedEstado;
                }
                else
                {
                    throw new ArgumentException($"Estado '{dto.Estado}' no válido. Valores: Pendiente, FueraDePlazo, Completada");
                }
            }

            _mapper.Map(dto, existing);
            if (!string.IsNullOrWhiteSpace(dto.Estado) && Enum.TryParse<TareaEstado>(dto.Estado, ignoreCase: true, out var estado2))
                existing.Estado = estado2;

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
            dtoResp.Estado = updated.Estado.ToString();
            dtoResp.Overdue = IsOverdue(updated, domPlantilla);

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

            // Validación: usuario (OPCIONAL)
            if (!string.IsNullOrWhiteSpace(dto.UsuarioEspacioId))
            {
                var userValidation = await _ptservice.ValidateUsuarioEspacioExistAsync(espacioid, dto.UsuarioEspacioId);
                if (!userValidation.IsValid)
                    throw new InvalidOperationException(userValidation.ErrorMessage);
            }

            var domPlantilla = plantilla.Adapt<PlantillaTarea>();

            // Parse estado si viene (string -> enum)
            if (!string.IsNullOrWhiteSpace(dto.Estado))
            {
                if (Enum.TryParse<TareaEstado>(dto.Estado, ignoreCase: true, out var parsedEstado))
                {
                    if (parsedEstado == TareaEstado.Pendiente)
                    {
                        var isOverdue = IsOverdue(existing, domPlantilla);
                        if (isOverdue)
                            throw new InvalidOperationException("No se puede marcar como pendiente una tarea que está overdue.");
                    }
                }
                else
                {
                    throw new ArgumentException($"Estado '{dto.Estado}' no válido. Valores: Pendiente, FueraDePlazo, Completada");
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
                dtoResp.HoraLimite = plantilla.HoraLimite;
                dtoResp.FacturaId = plantilla.FacturaId;
                dtoResp.Estado = current.Estado.ToString();
                dtoResp.Overdue = IsOverdue(current, domPlantilla);
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
            dtoResult.Estado = updated.Estado.ToString();
            dtoResult.Overdue = IsOverdue(updated, domPlantilla);

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

            // Parse Estado: string -> enum -> store as string name
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
                TareaEstado.Completada => tarea.Estado == TareaEstado.Completada,
                TareaEstado.Pendiente => tarea.Estado == TareaEstado.Pendiente && !IsOverdue(tarea, plantilla),
                TareaEstado.FueraDePlazo => tarea.Estado == TareaEstado.FueraDePlazo || (tarea.Estado != TareaEstado.Completada && IsOverdue(tarea, plantilla)),
                _ => false
            };
        }
    }
}