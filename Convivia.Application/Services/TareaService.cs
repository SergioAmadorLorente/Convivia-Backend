using Convivia.Application.Mappers;
using Convivia.Application.Services;
using Convivia.Domain.Entities;
using Convivia.Domain.Models;
using Convivia.Domain.Repositories;
using Convivia.Infrastructure.Services;
using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Google.Apis.Util;
using Mapster;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            _ptservice = ptservice ?? throw new ArgumentNullException(nameof(_mapper));
        }

        public async Task<string> AddAsync(string espacioid, CreateTareaDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var createPlantilla = _mapper.Map<CreatePlantillaTareaDto>(dto);

            var plantillaId = await _ptservice.AddAsync(createPlantilla);

            var tarea = _mapper.Map<Tarea>(dto);
            tarea.EspacioId = espacioid;
            tarea.PlantillaId = plantillaId;

            var id = await _repository.AddAsync(tarea);

            var plantilla = await _ptservice.GetByIdAsync(plantillaId);
            if (plantilla != null)
            {
                plantilla.TareasId.Add(id);
                await _ptservice.UpdateAsync(plantilla.PlantillaId, new UpdatePlantillaTareaDto { Nombre = plantilla.Nombre, karma = plantilla.karma, DiasRepeticion = plantilla.DiasRepeticion });
            }

            return id;
        }

        public async Task<IEnumerable<TareaDto>> GetAsync()
        {
            var tareas = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<TareaDto>>(tareas);
        }

        public async Task<IEnumerable<TareaDto>> GetAllByEspacioAsync(string espacioid)
        {
            var tareas = await _repository.GetAllByEspacioIdAsync(espacioid);
            return _mapper.Map<IEnumerable<TareaDto>>(tareas);
        }
        

        public async Task<TareaDto?> GetByIdAsync(string espacioid, string id)
        {
            var tarea = await _repository.GetAsync(espacioid, id);
            if (tarea == null) return null;
            return _mapper.Map<TareaDto>(tarea);
        }

        public async Task<TareaDto> UpdateAsync(string espacioid, string id, UpdateTareaDto dto)
        {
            var tarea = await _repository.GetAsync(espacioid, id);
            if (tarea == null) throw new ArgumentNullException(nameof(tarea));
            tarea = _mapper.Map<Tarea>(dto);

            if (!string.IsNullOrWhiteSpace(dto.PlantillaId)) tarea.PlantillaId = dto.PlantillaId;
            if (!string.IsNullOrWhiteSpace(dto.SalaId)) tarea.SalaId = dto.SalaId;

            await _repository.UpdateAsync(id, tarea);
            var tareaDto = _mapper.Map<TareaDto>(tarea);
            return tareaDto;
        }

        public async Task<bool> DeleteAsync(string espacioid, string id)
        {
            var tarea = await _repository.GetAsync(espacioid, id);
            if (tarea == null) return false;
            await _repository.DeleteAsync(id);
            return true;
        }
    }
}