using Convivia.Application.DTOs;
using Convivia.Application.Mappers;
using Convivia.Shared.Services;
using Convivia.Shared.Repositories;
using Convivia.Domain.Models;
using MapsterMapper;

namespace Convivia.Application.Services
{
    public class PlantillaTareaService
    {
        private readonly IPlantillaTareaRepository<PlantillaTarea> _repository;
        private readonly IMapper _mapper;

        public PlantillaTareaService(IPlantillaTareaRepository<PlantillaTarea> plantilla, IMapper _mapper)
        {
            _repository = plantilla ?? throw new ArgumentNullException(nameof(plantilla));
            this._mapper = _mapper ?? throw new ArgumentNullException(nameof(_mapper));
        }

        public async Task<PlantillaTareaDto> AddAsync(CreatePlantillaTareaDto dto)
        {

            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var plantillaTarea = _mapper.Map<PlantillaTarea>(dto);
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

    }
}