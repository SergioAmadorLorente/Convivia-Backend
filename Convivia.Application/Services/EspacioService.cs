// Application/Services/EspacioService.cs
using Convivia.Domain.Entities;
using Convivia.Domain.Repositories;
using Convivia.Shared.Helpers;
using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Application.Services
{
    public class EspacioService
    {
        private readonly IPlantillaTareaRepository _plantillaTareaRepo;
        private readonly IEspacioRepository _repo;
        private readonly IUsuarioEspacioRepository _usuarioEspacioRepo;
        private readonly ILogger<EspacioService> _logger;

        public EspacioService(
            IPlantillaTareaRepository plantillaTareaRepo,
            IEspacioRepository repo,
            IUsuarioEspacioRepository usuarioEspacioRepo,
            ILogger<EspacioService> logger)
        {
            _plantillaTareaRepo = plantillaTareaRepo ?? throw new ArgumentNullException(nameof(plantillaTareaRepo));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _usuarioEspacioRepo = usuarioEspacioRepo ?? throw new ArgumentNullException(nameof(usuarioEspacioRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> CrearAsync(CreateEspacioDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("Nombre requerido");

            var espacio = dto.Adapt<EspacioDto>();
            espacio.Id = Guid.NewGuid().ToString("N");
            return await _repo.AddAsync(espacio, ct);
        }

        public async Task<EspacioDto?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return await _repo.GetByIdAsync(id, ct);
        }

        public async Task ActualizarAsync(string id, CreateEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido");
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var espacioExistente = await _repo.GetByIdAsync(id, ct);
            if (espacioExistente == null) throw new KeyNotFoundException($"Espacio con Id {id} no encontrado");

            // Si el Id no cambia, actualizar campos normales
            espacioExistente.Nombre = dto.Nombre ?? espacioExistente.Nombre;
            espacioExistente.Direccion = dto.Direccion ?? espacioExistente.Direccion;

            await _repo.UpdateAsync(id, espacioExistente, ct);
        }

        /// <summary>
        /// Eliminar espacio con política RESTRICT: si existen UsuarioEspacio asociados, lanzar InvalidOperationException.
        /// </summary>
        public async Task EliminarAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido", nameof(id));

            // 1) RESTRICT: evita materializar colecciones grandes
            if (await _usuarioEspacioRepo.ExistsByEspacioIdAsync(id, ct).ConfigureAwait(false))
                throw new InvalidOperationException($"No se puede eliminar el espacio {id}: existen usuarios asociados.");

            // 2) Obtener espacio y plantillas (idempotente si no existe)
            var espacio = await _repo.GetByIdAsync(id, ct).ConfigureAwait(false);
            if (espacio == null) return;

            var plantillas = await _plantillaTareaRepo.GetByUsuarioEspacioIdAsync(espacio.Id, ct).ConfigureAwait( false);

            // 3) Borrar plantillas en batches (BatchHelper maneja reintentos)
            await BatchHelper.ProcessInBatchesAsync(
                plantillas,
                async (batch, token) =>
                {
                    var deletes = batch.Select(p => _plantillaTareaRepo.DeleteAsync(p.Id, token));
                    await Task.WhenAll(deletes).ConfigureAwait(false);
                },
                batchSize: 200,
                maxRetries: 3,
                initialRetryDelay: TimeSpan.FromSeconds(1),
                logger: _logger,
                ct: ct
            ).ConfigureAwait(false);

            // 4) Borrar espacio al final
            await _repo.DeleteAsync(id, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Cambia el Id de un Espacio y propaga el nuevo Id a todos los UsuarioEspacio (ON UPDATE CASCADE).
        /// Nota: renombrar Id en Firestore implica crear nuevo documento y borrar el antiguo.
        /// </summary>
        public async Task ChangeEspacioIdCascadeAsync(string oldId, string newId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(oldId)) throw new ArgumentException("oldId requerido");
            if (string.IsNullOrWhiteSpace(newId)) throw new ArgumentException("newId requerido");
            if (oldId == newId) return;

            var espacio = await _repo.GetByIdAsync(oldId, ct);
            if (espacio == null) throw new KeyNotFoundException($"Espacio {oldId} no encontrado");

            // 1) Crear nuevo documento con newId
            espacio.Id = newId;
            await _repo.AddAsync(espacio, ct);

            // 2) Obtener todos los UsuarioEspacio que referencian oldId
            var usuarios = (await _usuarioEspacioRepo.GetByEspacioIdAsync(oldId, ct)).ToList();

            // 3) Actualizar en batches usando BatchHelper para no saturar
            await BatchHelper.ProcessInBatchesAsync<UsuarioEspacio>(
                usuarios,
                async (batch, token) =>
                {
                    foreach (var ue in batch)
                    {
                        ue.EspacioId = newId;
                        await _usuarioEspacioRepo.UpdateAsync(ue.Id_UsuarioEspacio, ue, token);
                    }
                },
                batchSize: 500,
                maxRetries: 3,
                ct: ct
            );

            // 4) Borrar documento antiguo
            await _repo.DeleteAsync(oldId, ct);
        }

        public async Task<IEnumerable<EspacioDto>> ObtenerPorDireccionAsync(string direccion, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(direccion)) return Enumerable.Empty<EspacioDto>();
            try
            {
                return await _repo.GetByDireccionAsync(direccion, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorDireccion {Direccion}", direccion);
                throw;
            }
        }

        public async Task<bool> ParcialActualizarAsync(string id, UpdateEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido");
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            try
            {
                var espacio = await _repo.GetByIdAsync(id, ct);
                if (espacio == null) return false;

                var changed = false;

                if (!string.IsNullOrWhiteSpace(dto.Nombre) && dto.Nombre != espacio.Nombre)
                {
                    espacio.Nombre = dto.Nombre;
                    changed = true;
                }

                if (dto.Direccion != null && dto.Direccion != espacio.Direccion)
                {
                    espacio.Direccion = dto.Direccion;
                    changed = true;
                }

                if (!changed) return true;

                await _repo.UpdateAsync(id, espacio, ct);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parcial actualizando espacio {Id}", id);
                throw;
            }
        }
    }
}