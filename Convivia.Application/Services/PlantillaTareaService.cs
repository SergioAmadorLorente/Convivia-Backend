using Convivia.Application.Mappers;
using Convivia.Shared.Services;
using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;
using Convivia.Domain.Entities;
using MapsterMapper;
using Convivia.Domain.Repositories;
using Mapster;

namespace Convivia.Application.Services
{
    public class PlantillaTareaService
    {
        private readonly IPlantillaTareaRepository _repository;
        private readonly IMapper _mapper;

        public PlantillaTareaService(IPlantillaTareaRepository plantilla, IMapper _mapper)
        {
            _repository = plantilla ?? throw new ArgumentNullException(nameof(plantilla));
            this._mapper = _mapper ?? throw new ArgumentNullException(nameof(_mapper));
        }

        public async Task<string> AddAsync(CreatePlantillaTareaDto dto, string espacioid)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var plantillaTarea = _mapper.Map<PlantillaTarea>(dto);
            if (espacioid != null) { 
                plantillaTarea.EspacioId = espacioid;
            }
            var plantillanovaid = await _repository.AddAsync(plantillaTarea);
            return plantillanovaid;
        }

        public async Task<PlantillaTareaDto> UpdateAsync(string espacioid, string id, UpdatePlantillaTareaDto dto)
        {
            var plantilla = await GetByEspacioAndIdAsync(espacioid, id);
            if (plantilla == null) throw new ArgumentNullException(nameof(plantilla));

            var domPlantilla = plantilla.Adapt<PlantillaTarea>();

            await _repository.UpdateAsync(id, domPlantilla);

            return dto.Adapt<PlantillaTareaDto>();
        }

        public async Task<bool> DeleteAsync(string espacioid, string id)
        {
            var plantilla = await _repository.GetByEspacioAndIdAsync(espacioid, id);
            if (plantilla == null) return false;
            await _repository.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<PlantillaTareaDto>> GetAllByEspacioAsync(string espacioid)
        {
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
            var plantilla = await _repository.GetAsync(id);
            if (plantilla == null) return null;
            return _mapper.Map<PlantillaTareaDto>(plantilla);
        }

        public async Task<PlantillaTareaDto?> GetByEspacioAndIdAsync(string espacioid, string id)
        {
            var plantilla = await _repository.GetByEspacioAndIdAsync(espacioid, id);
            if (plantilla == null) return null;
            return _mapper.Map<PlantillaTareaDto>(plantilla);
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