using Convivia.Application.DTOs;
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
            var tarea = _mapper.Map<Tarea>(dto);
            // falta crear també plantillatarea i enllaçar-la
            var id = await _repository.AddAsync(tarea);
            return id;

        }

        public async Task<IEnumerable<TareaDto>> GetAsync(string espacioid)
        {
            var tareas = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<TareaDto>>(tareas);
        }

        public async Task<TareaDto?> GetByIdAsync(string espacioid, string id)
        {

            var tarea = await _repository.GetAsync(id);
            if (tarea == null) return null;
            return _mapper.Map<TareaDto>(tarea);

        }

        public async Task<TareaDto> UpdateAsync(string espacioid, string id, UpdateTareaDto dto)
        {

            var tarea = await _repository.GetAsync(id);
            if (tarea == null) throw new ArgumentNullException(nameof(tarea));

            tarea.Nombre = dto.Nombre ?? tarea.Nombre;
            tarea.PuntosKarma = dto.PuntosKarma ?? tarea.PuntosKarma;
            tarea.Estado = dto.Estado ?? tarea.Estado;
            tarea.HoraLimite = dto.HoraLimite ?? tarea.HoraLimite;
            tarea.UsuarioEspaciosIds = dto.UsuarioEspacioIds ?? tarea.UsuarioEspaciosIds;
            tarea.Foto = dto.Foto ?? tarea.Foto;
            tarea.Prorroga = dto.Prorroga ?? tarea.Prorroga;
            tarea.FacturaId = dto.FacturaId ?? tarea.FacturaId;

            // falta descripcio enllaçar en plantillatarea, i eliminar nullable plantillaid als dtos

            await _repository.UpdateAsync(id, tarea);
            var tareaDto = _mapper.Map<TareaDto>(tarea);
            return tareaDto;

        }

        public async Task<bool> DeleteAsync(string espacioid, string id)
        {
            var tarea = await _repository.GetAsync(id);
            if (tarea == null) return false;
            await _repository.DeleteAsync(id);
            return true;
        }
    }
}