using Convivia.Domain.Entities;
using Convivia.Domain.Repositories;
using Convivia.Shared.DTOs;
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

            var plantillaytarea = _mapper.Map<TareaDto>(tarea);

            plantillaytarea.Nombre = plantilla.Nombre;
            plantillaytarea.Descripcion = plantilla.Descripcion;

            return plantillaytarea;
        }

        public async Task<bool> DeleteAsync(string espacioid, string id)
        {
            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, id);
            if (plantilla == null) return false;
            var resultat = await _ptservice.DeleteAsync(id);
            return resultat;

        }

        // TODO

        public async Task<TareaDto> UpdateAsync(string espacioid, string id, UpdateTareaDto dto)
        {
            var plantilla = await _ptservice.GetByEspacioAndIdAsync(espacioid, id);
            if (plantilla == null) throw new ArgumentNullException(nameof(plantilla));
            var tareas = await _repository.GetAllAsync(id);
            var plantillaupdatedto = _mapper.Map<UpdatePlantillaTareaDto>(dto);
            var tareaactualizada = _mapper.Map<Tarea>(dto);
            await _ptservice.UpdateAsync(id, plantillaupdatedto);
            await _repository.UpdateAsyncList(tareas, tareaactualizada);
            return _mapper.Map<TareaDto>(plantillaupdatedto);
        }

    }
}