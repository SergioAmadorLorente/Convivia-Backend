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

        public async Task<string> AddAsync(CreatePlantillaTareaDto dto)
        {

            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var plantillaTarea = _mapper.Map<PlantillaTarea>(dto);
            plantillaTarea.PlantillaId = Guid.NewGuid().ToString("N");
            await _repository.AddAsync(plantillaTarea);
            return _mapper.Map<PlantillaTareaDto>(plantillaTarea);

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

        public async Task<PlantillaTareaDto> UpdateAsync(string id, UpdatePlantillaTareaDto dto)
        {

            var plantilla = await _repository.GetAsync(id);
            if (plantilla == null) throw new ArgumentNullException(nameof(plantilla));

            plantilla.Nombre = dto.Nombre ?? plantilla.Nombre;
            plantilla.karma = dto.karma ?? plantilla.karma;
            plantilla.Estado = dto.Estado ?? plantilla.Estado;
            plantilla.DiasRepeticion = dto.DiasRepeticion ?? plantilla.DiasRepeticion;
            await _repository.UpdateAsync(id, plantilla);

            var plantillaDto = _mapper.Map<PlantillaTareaDto>(plantilla);
            return plantillaDto;

        }

        public async Task<bool> DeleteAsync(string id)
        {
            var plantilla = await _repository.GetAsync(id);
            if (plantilla == null) return false;
            await _repository.DeleteAsync(id);
            return true;
        }

    }
}