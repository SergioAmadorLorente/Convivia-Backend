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


        public async Task<List<string>> AddAsync(string espacioid, CreateTareaDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var createPlantilla = _mapper.Map<CreatePlantillaTareaDto>(dto);

            var plantillaId = await _ptservice.AddAsync(createPlantilla, espacioid);

            var tareas = new List<Tarea>();

            foreach (int dia in createPlantilla.DiasRepeticion)
            {
                if (dia < 0 || dia > 6) throw new ArgumentException("DiasRepeticion debe contener valores entre 0 y 6 (0=Domingo, 6=Sábado).");

                var tarea = _mapper.Map<Tarea>(dto);
                tarea.PlantillaId = plantillaId;
                tarea.DiaSemana = dia;
                tarea.Completada = false;      // recién creada: no completada
                tarea.Disponible = true;   // recién creada: se puede hacer

                tareas.Add(tarea);
            }

            var ids = await _repository.AddAsyncList(tareas);

            var plantilla = await _ptservice.GetByIdAsync(plantillaId);
            if (plantilla != null)
            {
                foreach (var id in ids)
                    plantilla.TareasId.Add(id);

                await _ptservice.UpdateAsync(plantilla.PlantillaId, new UpdatePlantillaTareaDto
                {
                    Nombre = plantilla.Nombre,
                    karma = plantilla.karma,
                    DiasRepeticion = plantilla.DiasRepeticion,
                    TareasId = plantilla.TareasId
                });
            }

            return ids;
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

            // Derivado: overdue
            dto.Overdue = IsOverdue(tarea, plantilla.Adapt<PlantillaTarea>());

            // En la respuesta, si está overdue, no es disponible (aunque esté marcado en BD)
            if (dto.Overdue)
                dto.Disponible = false;

            return dto;
        }


        public async Task<bool> DeleteAsync(string espacioid, string id)
        {
            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, id);
            if (plantilla == null) return false;
            var resultat = await _ptservice.DeleteAsync(id);
            return resultat;

        }

        // TODO - delete pantalla edicio al canviar els dies en que es repeteix la tasca.

        public async Task<TareaDto> UpdateAsync(string espacioid, string id, UpdateTareaDto dto)
        {
            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, id);
            if (plantilla == null) throw new ArgumentNullException(nameof(plantilla));

            var tareas = await _repository.GetAllAsync(id);
            if (tareas is null || tareas.Count == 0)
                throw new InvalidOperationException("No hay tareas asociadas a la plantilla.");

            // 1) BLOQUEO: no permitir Disponible=true si alguna está overdue
            var domPlantilla = plantilla.Adapt<PlantillaTarea>();
            if (dto.Disponible == true)
            {
                var algunaOverdue = tareas.Any(t => IsOverdue(t, domPlantilla));
                if (algunaOverdue)
                    throw new InvalidOperationException("La tarea está vencida (overdue); no se puede marcar como disponible.");
            }

            // 2) Actualización de PLANTILLA
            var plantillaupdatedto = new UpdatePlantillaTareaDto
            {
                Nombre = dto.Nombre ?? plantilla.Nombre,
                karma = dto.karma ?? plantilla.karma,
                DiasRepeticion = plantilla.DiasRepeticion,
                TareasId = plantilla.TareasId,
                HoraLimite = dto.HoraLimite ?? plantilla.HoraLimite
            };

            // 3) Actualización parcial de TAREA
            var tareaactualizada = new Tarea
            {
                Nombre = dto.Nombre ?? tareas[0].Nombre,
                karma = dto.karma ?? tareas[0].karma,
                Disponible = dto.Disponible ?? tareas[0].Disponible,
                Completada = dto.Completada ?? tareas[0].Completada,
                UsuarioEspaciosIds = dto.UsuarioEspaciosIds ?? tareas[0].UsuarioEspaciosIds,
                FechaRealizacion = dto.FechaRealizacion ?? tareas[0].FechaRealizacion,
                Foto = dto.Foto ?? tareas[0].Foto,
                Prorroga = dto.Prorroga ?? tareas[0].Prorroga,
                FacturaId = dto.FacturaId ?? tareas[0].FacturaId,
                DiaSemana = tareas[0].DiaSemana,
                SalaId = dto.SalaId ?? tareas[0].SalaId,
                PlantillaId = tareas[0].PlantillaId
            };

            await _ptservice.UpdateAsync(id, plantillaupdatedto);
            await _repository.UpdateAsyncList(tareas, tareaactualizada);

            // 4) Recarga y calcula Overdue
            var tareaRecargada = await _repository.GetAsync(id, tareas[0].Id!);
            var dtoResp = _mapper.Map<TareaDto>(tareaRecargada);
            dtoResp.Nombre = plantillaupdatedto.Nombre!;
            dtoResp.Descripcion = plantilla.Descripcion;

            // Recalcular overdue con la plantilla actualizada
            var domPlantillaResp = plantilla.Adapt<PlantillaTarea>();
            domPlantillaResp.HoraLimite = plantillaupdatedto.HoraLimite ?? domPlantilla.HoraLimite;
            dtoResp.Overdue = IsOverdue(tareaRecargada, domPlantillaResp);

            if (dtoResp.Overdue)
                dtoResp.Disponible = false;

            return dtoResp;
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