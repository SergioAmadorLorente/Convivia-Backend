using Convivia.Application.DTOs;
using Convivia.Application.Mappers;
using Convivia.Application.Services;
using Convivia.Domain.Models;
using Convivia.Infrastructure.Services;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Google.Apis.Util;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convivia.Application.Services
{
    public class TareaService
    {
        private readonly ITareaRepository<Tarea> _repository;
        private readonly IMapper _mapper;
        private readonly PlantillaTareaService _ptservice;

        public TareaService(ITareaRepository<Tarea> tarea, IMapper _mapper, PlantillaTareaService ptservice)
        {
            _repository = tarea ?? throw new ArgumentNullException(nameof(tarea));
            this._mapper = _mapper ?? throw new ArgumentNullException(nameof(_mapper));
            _ptservice = ptservice ?? throw new ArgumentNullException(nameof(_mapper));
        }

        public async Task<TareaDto> AddAsync(CreateTareaDto dto)
        {

            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var Tarea = _mapper.Map<Tarea>(dto);
            Tarea.Id = Guid.NewGuid().ToString();
            var plantilla = _mapper.Map<CreatePlantillaTareaDto>(dto);
            await _ptservice.AddAsync(plantilla);
            await _repository.AddAsync(Tarea);
            return _mapper.Map<TareaDto>(Tarea);

        }

        public async Task<IEnumerable<TareaDto>> GetAsync()
        {
            var tareas = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<TareaDto>>(tareas);
        }

        public async Task<TareaDto?> GetByIdAsync(string id)
        {

            var tarea = await _repository.GetAsync(id);
            if (tarea == null) return null;
            return _mapper.Map<TareaDto>(tarea);

        }

        public async Task<TareaDto> UpdateAsync(string id, UpdateTareaDto dto)
        {

            var tarea = await _repository.GetAsync(id);
            if (tarea == null) throw new ArgumentNullException(nameof(tarea));

            tarea.Nombre = dto.Nombre ?? tarea.Nombre;
            tarea.PuntosKarma = dto.PuntosKarma ?? tarea.PuntosKarma;
            tarea.Estado = dto.Estado ?? tarea.Estado;
            await _repository.UpdateAsync(tarea);
            var tareaDto = _mapper.Map<TareaDto>(tarea);
            return tareaDto;

        }

        public async Task<bool> DeleteAsync(string id)
        {
            var tarea = await _repository.GetAsync(id);
            if (tarea == null) return false;
            await _repository.DeleteAsync(id);
            return true;
        }
    }
}