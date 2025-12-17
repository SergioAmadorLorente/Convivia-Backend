using Convivia.Domain.Entities;
using Convivia.Domain.Repositories;
using Convivia.Shared.DTOs;
using Mapster;
using MapsterMapper;

namespace Convivia.Application.Services
{
    public class TareaService
    {
        private readonly ITareaRepository _repository;
        private readonly IMapper _mapper;
        private readonly PlantillaTareaService _ptservice;

        public TareaService(ITareaRepository tarea, IMapper _mapper, PlantillaTareaService ptservice)
        {
            _repository = tarea ?? throw new ArgumentNullException(nameof(tarea));
            this._mapper = _mapper ?? throw new ArgumentNullException(nameof(_mapper));
            _ptservice = ptservice ?? throw new ArgumentNullException(nameof(ptservice));
        }


        public async Task<string> AddAsync(string espacioid, CreateTareaDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var createPlantilla = _mapper.Map<CreatePlantillaTareaDto>(dto);

            var tareas = new List<Tarea>();

            foreach (int dia in createPlantilla.DiasRepeticion)
            {
                if (dia < 0 || dia > 6) throw new ArgumentException("DiasRepeticion debe contener valores entre 0 y 6 (0=Domingo, 6=Sábado).");

                var tarea = _mapper.Map<Tarea>(dto);
                tarea.DiaSemana = dia;
                tarea.Completada = false;      // recién creada: no completada
                tarea.Disponible = true;   // recién creada: se puede hacer
                createPlantilla.TareasId.Add(tarea.Id);

                tareas.Add(tarea);
            }

            var plantillaId = await _ptservice.AddAsync(createPlantilla, espacioid);

            foreach (var tarea in tareas)
            {

                tarea.PlantillaId = plantillaId;

            }

            var ids = await _repository.AddAsyncList(tareas);

            return plantillaId;
        }

        public async Task<IEnumerable<PlantillaTareaDto>> GetAllByEspacioAsync(string espacioid)
        {
            var pttareas = await _ptservice.GetAllByEspacioAsync(espacioid);
            if (pttareas.Any() == false)
            {
                return Enumerable.Empty<PlantillaTareaDto>();
            }
            return _mapper.Map<IEnumerable<PlantillaTareaDto>>(pttareas);
        }

        public async Task<PlantillaTareaDto> GetByEspacioAndIdAsync(string espacioid, string id)
        {
            var pttarea = await _ptservice.GetByEspacioAndIdAsync(espacioid, id);
            if (pttarea == null) throw new ArgumentException("La plantilla no existe o no pertenece al espacio especificado.", nameof(id));
            return _mapper.Map<PlantillaTareaDto>(pttarea);
        }

        public async Task<TareaDto?> GetByEspacioAndPlantillaAndTareaAsync(string espacioid, string plantillaId, string tareaId)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new ArgumentNullException(nameof(plantillaId));
            if (string.IsNullOrWhiteSpace(tareaId)) throw new ArgumentNullException(nameof(tareaId));

            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, plantillaId);
            if (plantilla == null) return null;

            var tarea = await _repository.GetAsync(plantillaId, tareaId);
            if (tarea == null) return null;

            var dto = _mapper.Map<TareaDto>(tarea);

            // Complementa con datos de la plantilla
            dto.Nombre = plantilla.Nombre;
            dto.Descripcion = plantilla.Descripcion;
            dto.karma = plantilla.karma;
            dto.HoraLimite = plantilla.HoraLimite;
            dto.FacturaId = plantilla.FacturaId;

            // Derivado: overdue
            dto.Overdue = IsOverdue(tarea, plantilla.Adapt<PlantillaTarea>());

            // En la respuesta, si está overdue, no es disponible (aunque esté marcado en BD)
            if (dto.Overdue)
                dto.Disponible = false;

            return dto;
        }

        // Overwrite (PUT): replace entire tarea document
        public async Task<TareaDto?> UpdateCompleteAsync(string espacioid, string plantillaid, string tareaid, UpdateTareaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(plantillaid)) throw new ArgumentNullException(nameof(plantillaid));
            if (string.IsNullOrWhiteSpace(tareaid)) throw new ArgumentNullException(nameof(tareaid));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, plantillaid);
            if (plantilla == null) return null;

            var existing = await _repository.GetAsync(plantillaid, tareaid, ct);
            if (existing == null) return null;

            // 1) BLOQUEO: no permitir Disponible=true si está overdue
            var domPlantilla = plantilla.Adapt<PlantillaTarea>();
            if (dto.Disponible == true)
            {
                var isOverdue = IsOverdue(existing, domPlantilla);
                if (isOverdue)
                    throw new InvalidOperationException("No se puede marcar como disponible una tarea que está overdue.");
            }

            // Map DTO -> Domain (new object)
            var domain = _mapper.Map<Tarea>(dto);
            domain.Id = tareaid;
            domain.PlantillaId = plantillaid;

            // Persist overwrite
            await _repository.UpdateAsync(tareaid, domain, merge: false, ct);

            var updated = await _repository.GetAsync(plantillaid, tareaid, ct);
            if (updated == null) return null;

            var dtoResp = _mapper.Map<TareaDto>(updated);
            dtoResp.Nombre = plantilla.Nombre!;
            dtoResp.Descripcion = plantilla.Descripcion;
            dtoResp.karma = plantilla.karma;
            dtoResp.HoraLimite = plantilla.HoraLimite;
            dtoResp.FacturaId = plantilla.FacturaId;

            dtoResp.Overdue = IsOverdue(updated, domPlantilla);
            if (dtoResp.Overdue) dtoResp.Disponible = false;

            return dtoResp;
        }

        // delete

        public async Task<bool> DeleteAsync(string espacioid, string plantillaid, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(plantillaid)) throw new ArgumentNullException(nameof(plantillaid));

            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, plantillaid);
            if (plantilla == null) return false;

            foreach (string tareaid in plantilla.TareasId)
            {

                 await _repository.DeleteAsync(tareaid);

            }

            var resultat = await _ptservice.DeleteAsync(espacioid, plantillaid);
            if (resultat == false) return false;

            return true;

        }

        // Merge (PUT merge): map DTO onto existing and persist with merge option
        public async Task<TareaDto?> UpdateMergeAsync(string espacioid, string plantillaid, string tareaid, UpdateTareaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(plantillaid)) throw new ArgumentNullException(nameof(plantillaid));
            if (string.IsNullOrWhiteSpace(tareaid)) throw new ArgumentNullException(nameof(tareaid));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, plantillaid);
            if (plantilla == null) return null;

            var existing = await _repository.GetAsync(plantillaid, tareaid, ct);
            if (existing == null) return null;

            // 1) BLOQUEO: no permitir Disponible=true si está overdue
            var domPlantilla = plantilla.Adapt<PlantillaTarea>();
            if (dto.Disponible == true)
            {
                var isOverdue = IsOverdue(existing, domPlantilla);
                if (isOverdue)
                    throw new InvalidOperationException("No se puede marcar como disponible una tarea que está overdue.");
            }

            // Map DTO onto existing (ignore nulls configured in Mapster)
            _mapper.Map(dto, existing);

            await _repository.UpdateAsync(tareaid, existing, merge: true, ct);

            var updated = await _repository.GetAsync(plantillaid, tareaid, ct);
            if (updated == null) return null;

            var dtoResp = _mapper.Map<TareaDto>(updated);
            dtoResp.Nombre = plantilla.Nombre!;
            dtoResp.Descripcion = plantilla.Descripcion;
            dtoResp.karma = plantilla.karma;
            dtoResp.HoraLimite = plantilla.HoraLimite;
            dtoResp.FacturaId = plantilla.FacturaId;

            dtoResp.Overdue = IsOverdue(updated, domPlantilla);
            if (dtoResp.Overdue) dtoResp.Disponible = false;

            return dtoResp;
        }

        // Partial (PATCH): build updates dictionary and call repository partial update
        public async Task<TareaDto?> UpdatePartialAsync(string espacioid, string plantillaid, string tareaid, UpdateTareaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioid)) throw new ArgumentNullException(nameof(espacioid));
            if (string.IsNullOrWhiteSpace(plantillaid)) throw new ArgumentNullException(nameof(plantillaid));
            if (string.IsNullOrWhiteSpace(tareaid)) throw new ArgumentNullException(nameof(tareaid));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, plantillaid);
            if (plantilla == null) return null;

            var existing = await _repository.GetAsync(plantillaid, tareaid, ct);
            if (existing == null) return null;

            // 1) BLOQUEO: no permitir Disponible=true si está overdue
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
                if (dtoResp.Overdue) dtoResp.Disponible = false;
                return dtoResp;
            }

            // useSetMerge: false -> strict update (will throw if not exists)
            await _repository.UpdateAsync(tareaid, updates, useSetMerge: false, ct);

            var updated = await _repository.GetAsync(plantillaid, tareaid, ct);
            if (updated == null) return null;

            var dtoResult = _mapper.Map<TareaDto>(updated);
            dtoResult.Nombre = plantilla.Nombre!;
            dtoResult.Descripcion = plantilla.Descripcion;
            dtoResult.karma = plantilla.karma;
            dtoResult.HoraLimite = plantilla.HoraLimite;
            dtoResult.FacturaId = plantilla.FacturaId;
            dtoResult.Overdue = IsOverdue(updated, domPlantilla);
            if (dtoResult.Overdue) dtoResult.Disponible = false;

            return dtoResult;
        }

        // Keep existing UpdateAsync for backward compatibility (acts like PATCH)
        public async Task<TareaDto> UpdateAsync(string espacioid, string plantillaid, string tareaid, UpdateTareaDto dto)
        {
            // Delegate to partial update
            var res = await UpdatePartialAsync(espacioid, plantillaid, tareaid, dto);
            return res!;
        }

        private IDictionary<string, object> ObtenerActualizacionesDesdeDto(UpdateTareaDto dto)
        {
            var updates = new Dictionary<string, object>();
            if (dto.UsuarioEspaciosIds != null && dto.UsuarioEspaciosIds.Count > 0) updates["UsuarioEspaciosIds"] = dto.UsuarioEspaciosIds;
            if (dto.FechaRealizacion.HasValue) updates["FechaRealizacion"] = dto.FechaRealizacion.Value;
            if (dto.Foto != null) updates["Foto"] = dto.Foto;
            if (dto.Prorroga.HasValue) updates["Prorroga"] = dto.Prorroga.Value;
            if (dto.Disponible.HasValue) updates["Disponible"] = dto.Disponible.Value;
            if (dto.Completada.HasValue) updates["Completada"] = dto.Completada.Value;
            if (!string.IsNullOrWhiteSpace(dto.SalaId)) updates["SalaId"] = dto.SalaId;
            return updates;
        }

        private static bool IsOverdue(Tarea tarea, PlantillaTarea plantilla)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(plantilla.TimeZoneId);
            var nowUtc = DateTime.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);

            // Si hoy no es el día de la tarea, no está overdue
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

    }
}