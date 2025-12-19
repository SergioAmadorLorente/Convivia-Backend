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
            // Nota: la asignación de usuarios se realiza mediante la lista UsuariosAsignacion (requerida).
            // Validaciones de usuarios concretas se harán más abajo al crear tareas.

            var createPlantilla = _mapper.Map<CreatePlantillaTareaDto>(dto);
            // Ensure the plantilla dto preserves the incoming lists from the front-end
            createPlantilla.DiasRepeticion = dto.DiasRepeticion ?? new List<int>();
            createPlantilla.UsuariosAsignacion = dto.UsuariosAsignacion ?? new List<string>();

            var tareas = new List<Tarea>();

            if (esPuntual)
            {
                // Tarea puntual: 1 sola tarea
                var tarea = _mapper.Map<Tarea>(dto);
                tarea.DiaSemana = -1; // Indicador de tarea puntual
                tarea.Estado = TareaEstado.Pendiente;

                // UsuariosAsignacion ahora es obligatorio: debe contener exactamente 1 elemento para tareas puntuales
                if (dto.UsuariosAsignacion == null || dto.UsuariosAsignacion.Count == 0)
                    throw new ArgumentException("Se requiere al menos un Usuario en UsuariosAsignacion para crear tareas.");

                if (dto.UsuariosAsignacion.Count != 1)
                    throw new ArgumentException($"Tarea puntual requiere exactamente 1 usuario en UsuariosAsignacion. Se recibieron {dto.UsuariosAsignacion.Count}.");

                // Respect the exact order / value sent by the front: assign the single provided user
                tarea.UsuarioEspacioId = dto.UsuariosAsignacion[0];
                tarea.FechaLimite = dto.FechaLimite;
                createPlantilla.TareasId.Add(tarea.Id!);
                tareas.Add(tarea);
            }
            else
            {
                // Tarea repetida: 1 tarea por día de repetición
                if (dto.UsuariosAsignacion == null || dto.UsuariosAsignacion.Count == 0)
                    throw new ArgumentException("UsuariosAsignacion es obligatorio y debe contener los usuarios a asignar a cada tarea.");

                var users = dto.UsuariosAsignacion;
                var days = dto.DiasRepeticion ?? new List<int>();
                var taskCount = days.Count;

                if (users.Count != 1 && users.Count != taskCount)
                    throw new ArgumentException($"Si envías más de un usuario, el número de usuarios ({users.Count}) debe coincidir con el número de tareas a crear ({taskCount}).");

                // Iterate using the front-provided days order and preserve the users order
                for (int i = 0; i < days.Count; i++)
                {
                    var dia = days[i];
                    var tarea = _mapper.Map<Tarea>(dto);
                    tarea.DiaSemana = dia;
                    tarea.Estado = TareaEstado.Pendiente;
                    tarea.FechaLimite = dto.FechaLimite;

                    // If single user -> same for all; otherwise assign according to the front-provided order
                    tarea.UsuarioEspacioId = users.Count == 1 ? users[0] : users[i];

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
                    dto.EsPuntual = tarea.DiaSemana == -1;
                    dto.UsuarioEspacioId = tarea.UsuarioEspacioId;

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
                    dto.EsPuntual = tarea.DiaSemana == -1;
                    dto.UsuarioEspacioId = tarea.UsuarioEspacioId;

                    tareasFiltered.Add(dto);
                }
            }

            return tareasFiltered;
        }

        /// <summary>
        /// Filtrar tareas por día y/o estado y/o usuario.
        /// </summary>
        public async Task<IEnumerable<TareaDto>> FilterAsync(string espacioid, int? diaSemana, string? estado, string? usuarioId = null)
        {
            if (string.IsNullOrWhiteSpace(espacioid))
                throw new ArgumentNullException(nameof(espacioid));

            if (!diaSemana.HasValue && string.IsNullOrWhiteSpace(estado) && string.IsNullOrWhiteSpace(usuarioId))
                throw new ArgumentException("Se requiere al menos uno de: 'diaSemana', 'estado' o 'usuarioId'.");

            if (diaSemana.HasValue && (diaSemana < 0 || diaSemana > 6))
                throw new ArgumentException("diaSemana debe estar entre 0 y 6.");

            TareaEstado? parsedEstado = null;
            if (!string.IsNullOrWhiteSpace(estado))
            {
                if (!Enum.TryParse<TareaEstado>(estado, ignoreCase: true, out var p))
                    throw new ArgumentException("estado no válido. Valores válidos: Pendiente, FueraDePlazo, Completada");
                parsedEstado = p;
            }

            // Delegate to FilterByMultipleCriteriaAsync which handles all combinations
            return await FilterByMultipleCriteriaAsync(espacioid, diaSemana, parsedEstado, usuarioId);
        }

        /// <summary>
        /// Filtra tareas por múltiples criterios: diaSemana, estado y/o usuarioId.
        /// </summary>
        private async Task<IEnumerable<TareaDto>> FilterByMultipleCriteriaAsync(
            string espacioid, 
            int? diaSemana, 
            TareaEstado? estado,
            string? usuarioId)
        {
            var pttareas = await _ptservice.GetAllByEspacioAsync(espacioid);
            var tareasFiltered = new List<TareaDto>();

            foreach (var plantilla in pttareas)
            {
                var pt = plantilla.Adapt<PlantillaTarea>();

                foreach (var tareaId in plantilla.TareasId ?? new List<string>())
                {
                    var tarea = await _repository.GetAsync(plantilla.PlantillaId, tareaId);
                    if (tarea == null) continue;

                    // Filter by day if provided
                    if (diaSemana.HasValue && tarea.DiaSemana != diaSemana)
                        continue;

                    // Filter by state if provided
                    if (estado.HasValue && !MatchesEstado(tarea, pt, estado.Value))
                        continue;

                    // Filter by usuario if provided
                    if (!string.IsNullOrWhiteSpace(usuarioId) && tarea.UsuarioEspacioId != usuarioId)
                        continue;

                    var dto = _mapper.Map<TareaDto>(tarea);
                    dto.Nombre = plantilla.Nombre;
                    dto.Descripcion = plantilla.Descripcion;
                    dto.karma = plantilla.karma;
                    dto.HoraLimite = plantilla.HoraLimite;
                    dto.FacturaId = plantilla.FacturaId;
                    dto.Estado = tarea.Estado.ToString();
                    dto.Overdue = IsOverdue(tarea, pt);
                    dto.EsPuntual = tarea.DiaSemana == -1;
                    dto.UsuarioEspacioId = tarea.UsuarioEspacioId;

                    tareasFiltered.Add(dto);
                }
            }

            return tareasFiltered;
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
            dto.EsPuntual = tarea.DiaSemana == -1;
            dto.UsuarioEspacioId = tarea.UsuarioEspacioId;

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
                        var isOverdue = IsOverdueWithUpdates(existing, domPlantilla, dto);
                        if (isOverdue)
                            throw new InvalidOperationException("No se puede marcar como pendiente una tarea que está overdue.");
                    }
                    // If marking as completed and no FechaRealizacion provided, force now
                    if (parsedEstado == TareaEstado.Completada && !dto.FechaRealizacion.HasValue)
                    {
                        dto.FechaRealizacion = DateTime.UtcNow;
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
            dtoResp.EsPuntual = updated.DiaSemana == -1;
            dtoResp.UsuarioEspacioId = updated.UsuarioEspacioId;

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
                        var isOverdue = IsOverdueWithUpdates(existing, domPlantilla, dto);
                        if (isOverdue)
                            throw new InvalidOperationException("No se puede marcar como pendiente una tarea que está overdue.");
                    }
                    // If marking as completed and no FechaRealizacion provided, force now
                    if (parsedEstado == TareaEstado.Completada && !dto.FechaRealizacion.HasValue)
                    {
                        dto.FechaRealizacion = DateTime.UtcNow;
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
            dtoResp.EsPuntual = updated.DiaSemana == -1;
            dtoResp.UsuarioEspacioId = updated.UsuarioEspacioId;

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
                        var isOverdue = IsOverdueWithUpdates(existing, domPlantilla, dto);
                        if (isOverdue)
                            throw new InvalidOperationException("No se puede marcar como pendiente una tarea que está overdue.");
                    }
                    // If marking as completed and no FechaRealizacion provided, force now
                    if (parsedEstado == TareaEstado.Completada && !dto.FechaRealizacion.HasValue)
                    {
                        dto.FechaRealizacion = DateTime.UtcNow;
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
                dtoResp.EsPuntual = current.DiaSemana == -1;
                dtoResp.UsuarioEspacioId = current.UsuarioEspacioId;
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
            dtoResult.EsPuntual = updated.DiaSemana == -1;
            dtoResult.UsuarioEspacioId = updated.UsuarioEspacioId;

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

            // If task already completed => not overdue
            if (tarea.FechaRealizacion.HasValue)
                return false;

            // If there is an explicit prorroga (extension), use it as the effective due datetime
            if (tarea.Prorroga.HasValue)
            {
                // Treat Prorroga as local in plantilla timezone; construct unspecified DateTime then convert
                var prorrogaLocal = new DateTime(
                    tarea.Prorroga.Value.Year,
                    tarea.Prorroga.Value.Month,
                    tarea.Prorroga.Value.Day,
                    tarea.Prorroga.Value.Hour,
                    tarea.Prorroga.Value.Minute,
                    tarea.Prorroga.Value.Second,
                    DateTimeKind.Unspecified);

                var dueUtcFromProrroga = new DateTimeOffset(prorrogaLocal, tz.GetUtcOffset(prorrogaLocal)).UtcDateTime;

                if (plantilla.GracePeriodMinutes.HasValue)
                    dueUtcFromProrroga = dueUtcFromProrroga.AddMinutes(plantilla.GracePeriodMinutes.Value);

                return nowUtc >= dueUtcFromProrroga;
            }

            // If punctual task (DiaSemana == -1) use FechaLimite + HoraLimite
            if (tarea.DiaSemana == -1)
            {
                if (!tarea.FechaLimite.HasValue)
                    return false;

                var fecha = tarea.FechaLimite.Value.Date;
                var horaLimite = plantilla.HoraLimite;

                var dueLocal = new DateTime(fecha.Year, fecha.Month, fecha.Day,
                                            horaLimite.Hour, horaLimite.Minute, 0, DateTimeKind.Unspecified);
                var dueUtc = new DateTimeOffset(dueLocal, tz.GetUtcOffset(dueLocal)).UtcDateTime;

                if (plantilla.GracePeriodMinutes.HasValue)
                    dueUtc = dueUtc.AddMinutes(plantilla.GracePeriodMinutes.Value);

                return nowUtc >= dueUtc;
            }

            // For repeated tasks, only consider overdue on the scheduled weekday
            if (tarea.DiaSemana != (int)nowLocal.DayOfWeek)
                return false;

            var occurrenceDate = nowLocal.Date;
            var horaLimiteRep = plantilla.HoraLimite;

            var dueLocalRep = new DateTime(occurrenceDate.Year, occurrenceDate.Month, occurrenceDate.Day,
                                        horaLimiteRep.Hour, horaLimiteRep.Minute, 0, DateTimeKind.Unspecified);
            var dueUtcRep = new DateTimeOffset(dueLocalRep, tz.GetUtcOffset(dueLocalRep)).UtcDateTime;

            if (plantilla.GracePeriodMinutes.HasValue)
                dueUtcRep = dueUtcRep.AddMinutes(plantilla.GracePeriodMinutes.Value);

            return nowUtc >= dueUtcRep;
        }

        // Helper that evaluates overdue considering incoming updates from UpdateTareaDto
        private static bool IsOverdueWithUpdates(Tarea existing, PlantillaTarea plantilla, UpdateTareaDto? dto)
        {
            // If no updates affecting due/completion, fall back to current computation
            if (dto == null)
                return IsOverdue(existing, plantilla);

            // Create a shallow clone of existing to apply potential updates
            var temp = existing.Adapt<Tarea>();

            if (dto.Prorroga.HasValue)
                temp.Prorroga = dto.Prorroga.Value;
            if (dto.FechaRealizacion.HasValue)
                temp.FechaRealizacion = dto.FechaRealizacion.Value;

            // If Estado is being set to Completada and FechaRealizacion provided, treat as completed
            if (!string.IsNullOrWhiteSpace(dto.Estado) && Enum.TryParse<TareaEstado>(dto.Estado, ignoreCase: true, out var parsed) && parsed == TareaEstado.Completada)
            {
                // If FechaRealizacion is provided, completion overrides overdue
                if (dto.FechaRealizacion.HasValue)
                    return false;
            }

            return IsOverdue(temp, plantilla);
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