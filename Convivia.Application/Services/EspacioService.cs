using Convivia.Application.Repositories;
using Convivia.Domain.Entities;
using Convivia.Shared.DTOs;
using Convivia.Shared.Services;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
// Application/Services/EspacioService.cs
using Convivia.Shared.Helpers;
using Mapster;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Convivia.Application.Services
{
    public class EspacioService
    {
        private readonly IEspacioRepository _espacioRepository;
        private readonly IMapper _mapper;
        private readonly IPlantillaTareaRepository _plantillaTareaRepo;
        private readonly IUsuarioEspacioRepository _usuarioEspacioRepo;
        private readonly ILogger<EspacioService> _logger;
        private readonly IMemoryCache _cache;
        private readonly UsuarioEspacioService _usuarioEspacioService;
        private readonly IUsuarioRepository _usuarioRepo;

        public EspacioService(
            IPlantillaTareaRepository plantillaTareaRepo,
            IUsuarioEspacioRepository usuarioEspacioRepo,
            ILogger<EspacioService> logger,
            IEspacioRepository espacioRepository, 
            IMapper mapper,
            IUsuarioRepository usuarioRepo,IMemoryCache cache, 
            UsuarioEspacioService usuarioEspacioService
        )
        {
            _espacioRepository = espacioRepository ?? throw new ArgumentNullException(nameof(espacioRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _usuarioRepo = usuarioRepo ?? throw new ArgumentNullException(nameof(usuarioRepo));
            _plantillaTareaRepo = plantillaTareaRepo ?? throw new ArgumentNullException(nameof(plantillaTareaRepo));
            _usuarioEspacioRepo = usuarioEspacioRepo ?? throw new ArgumentNullException(nameof(usuarioEspacioRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _usuarioEspacioService = usuarioEspacioService ?? throw new ArgumentNullException(nameof(usuarioEspacioService));
        }

        public async Task<EspacioDto> CrearEspacioAsync(CreateEspacioDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("Nombre requerido");

            // DTO -> domain
            var espacioDomain = _mapper.Map<Espacio>(dto);

            // Persistir y obtener id
            var id = await _espacioRepository.AddAsync(espacioDomain, ct);

            // Recuperar entidad guardada y devolver DTO consistente
            var createdDomain = await _espacioRepository.GetByIdAsync(id, ct);
            if (createdDomain == null)
            {
                // devolver DTO m�nimo con id para evitar fallos en rutas
                return new EspacioDto { Id = id };
            }

            var createdDto = _mapper.Map<EspacioDto>(createdDomain);
            if (string.IsNullOrWhiteSpace(createdDto.Id))
                createdDto.Id = id;

            return createdDto;
            
        }

        /// <summary>
        /// Obtiene una espacio por id.
        /// </summary>
        public async Task<EspacioDto?> ObtenerEspacioAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            var domain = await _espacioRepository.GetByIdAsync(id, ct);
            return domain == null ? null : _mapper.Map<EspacioDto>(domain);
        }


        /// <summary>
        /// Lista todas las espacios.
        /// </summary>
        public async Task<List<EspacioDto>> ListarTodasAsync(CancellationToken ct = default)
        {
            var list = await _espacioRepository.GetAllAsync(ct);
            return list?.Select(f => _mapper.Map<EspacioDto>(f)).ToList() ?? new List<EspacioDto>();
        }

        public async Task<IEnumerable<Espacio>> ObtenerPorDireccionAsync(string direccion, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(direccion)) return Enumerable.Empty<Espacio>();
            try
            {
                return await _espacioRepository.GetByDireccionAsync(direccion, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorDireccion {Direccion}", direccion);
                throw;
            }
        }

        // Overwrite completo: reemplaza todo el documento en Firestore (PUT)
        public async Task<EspacioDto?> ActualizarEspacioCompletoAsync(string id, UpdateEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var espacioexistente = await _espacioRepository.GetByIdAsync(id, ct);
            if (espacioexistente == null) return null;

            // Mapear DTO -> Domain (suponiendo que tienes la entidad Espacio)
            var domain = _mapper.Map<Espacio>(dto);
            domain.Id = id; // asegurar id

            // Persistir como overwrite 
            await _espacioRepository.UpdateAsync(id, domain, merge: false, ct);

            var updated = await _espacioRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<EspacioDto>(updated);
        }

        // Merge: mapear sobre la entidad existente y persistir con merge (Set + MergeAll)
        public async Task<EspacioDto?> ActualizarEspacioMergeAsync(string id, UpdateEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Obtener existente para mapear sobre �l y evitar perder campos no enviados
            var existing = await _espacioRepository.GetByIdAsync(id, ct);
            if (existing == null) return null;

            // Mapear solo valores no nulos (aseg�rate de configurar Mapster: IgnoreNullValues = true)
            _mapper.Map(dto, existing);

            await _espacioRepository.UpdateAsync(id, existing, merge: true, ct);

            var updated = await _espacioRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<EspacioDto>(updated);
        }

        // Parcial / PATCH: actualiza solo los campos permitidos (construye IDictionary<string, object>)
        public async Task<EspacioDto?> ActualizarEspacioParcialAsync(string id, UpdateEspacioDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Construir diccionario de actualizaciones (usa el helper que definiste)
            var updates = ObtenerActualizacionesDesdeDto(dto); // m�todo est�tico/privado que devuelve IDictionary<string, object>
            if (updates.Count == 0)
            {
                // Nada que actualizar: devolver la entidad actual
                var current = await _espacioRepository.GetByIdAsync(id, ct);
                return current == null ? null : _mapper.Map<EspacioDto>(current);
            }

          
            await _espacioRepository.UpdateAsync(id, updates, useSetMerge: false, ct);

            var updated = await _espacioRepository.GetByIdAsync(id, ct);
            return updated == null ? null : _mapper.Map<EspacioDto>(updated);             
        }


        /// <summary>
        /// Elimina una espacio.
        /// </summary>
        public async Task<bool> EliminarEspacioAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido", nameof(id));

            // 1) RESTRICT: evita materializar colecciones grandes
            if (await _usuarioEspacioRepo.ExistsByEspacioIdAsync(id, ct).ConfigureAwait(false))
                throw new InvalidOperationException($"No se puede eliminar el espacio {id}: existen usuarios asociados.");

            // 2) Obtener espacio y plantillas (idempotente si no existe)
            var espacio = await _espacioRepository.GetByIdAsync(id, ct).ConfigureAwait(false);
            if (espacio == null) return false;

            var plantillas = await _plantillaTareaRepo.GetByUsuarioEspacioIdAsync(espacio.Id, ct).ConfigureAwait(false);

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

            await _espacioRepository.DeleteAsync(id, ct);
            return true;
        }

        public async Task ChangeEspacioIdCascadeAsync(string oldId, string newId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(oldId)) throw new ArgumentException("oldId requerido");
            if (string.IsNullOrWhiteSpace(newId)) throw new ArgumentException("newId requerido");
            if (oldId == newId) return;

            var espacio = await _espacioRepository.GetByIdAsync(oldId, ct);
            if (espacio == null) throw new KeyNotFoundException($"Espacio {oldId} no encontrado");

            // 1) Crear nuevo documento con newId
            espacio.Id = newId;
            await _espacioRepository.AddAsync(espacio, ct);

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
                        await _usuarioEspacioRepo.UpdateAsync(ue.Id, ue, token);
                    }
                },
                batchSize: 500,
                maxRetries: 3,
                ct: ct
            );

            // 4) Borrar documento antiguo
            await _espacioRepository.DeleteAsync(oldId, ct);
        }


        /// <summary>
        /// Construye el diccionario de actualizaciones para PATCH de Espacio.
        /// - A�ade solo propiedades no nulas/no vac�as del DTO.
        /// - Usa los nombres exactos de Firestore seg�n FireStoreEspacio.
        /// - Filtra campos que no deben actualizarse por PATCH: "Id".
        /// - Normaliza cadenas (Trim) y aplica validaciones m�nimas (longitud).
        /// </summary>
        public static IDictionary<string, object> ObtenerActualizacionesDesdeDto(UpdateEspacioDto dto)
        {
            var updates = new Dictionary<string, object>();
            if (dto == null) return updates;

            // Nombre: actualizar solo si se env�a un valor no vac�o
            if (!string.IsNullOrWhiteSpace(dto.Nombre))
            {
                var nombre = dto.Nombre.Trim();
                const int maxNombre = 200;
                if (nombre.Length == 0)
                    throw new ArgumentException("Nombre no puede quedar vac�o.");
                if (nombre.Length > maxNombre)
                    throw new ArgumentException($"Nombre demasiado largo. M�ximo {maxNombre} caracteres.");
                updates["Nombre"] = nombre;
            }

            // Direccion: permitir cadena vac�a intencionada; excluir null
            if (dto.Direccion != null)
            {
                var direccion = dto.Direccion.Trim();
                const int maxDireccion = 1000;
                if (direccion.Length > maxDireccion)
                    throw new ArgumentException($"Direcci�n demasiado larga. M�ximo {maxDireccion} caracteres.");
                updates["Direccion"] = direccion;
            }

            return updates;
        }

        public async Task<string> GenerarCodigoInvitacionAsync(string espacioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentException("Id requerido");

            var espacio = await _espacioRepository.GetByIdAsync(espacioId, ct);
            if (espacio == null) throw new KeyNotFoundException($"Espacio con Id {espacioId} no encontrado");

            // Verificar si ya existe un c�digo v�lido para este espacio
            var espacioKey = $"EspacioId_{espacioId}";
            if (_cache.TryGetValue(espacioKey, out string codigoExistente))
            {
                _logger.LogInformation("C�digo de invitaci�n existente {Codigo} devuelto para espacio {EspacioId}", codigoExistente, espacioId);
                return codigoExistente;
            }

            // Generar un nuevo c�digo �nico
            string codigo;
            do
            {
                codigo = GenerarCodigoAleatorio();
            } while (_cache.TryGetValue($"InvitacionCode_{codigo}", out _));

            var cacheKey = $"InvitacionCode_{codigo}";
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            // Guardar el c�digo con el espacioId y viceversa
            _cache.Set(cacheKey, espacioId, cacheOptions);
            _cache.Set(espacioKey, codigo, cacheOptions);

            _logger.LogInformation("C�digo de invitaci�n {Codigo} generado para espacio {EspacioId}", codigo, espacioId);
            return codigo;
        }

        public async Task<UsuarioEspacioDto> UnirUsuarioPorCodigoAsync(string codigo, string usuarioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("C�digo requerido");
            if (string.IsNullOrWhiteSpace(usuarioId)) throw new ArgumentException("UsuarioId requerido");

            var cacheKey = $"InvitacionCode_{codigo}";
            if (!_cache.TryGetValue(cacheKey, out string espacioId))
            {
                _logger.LogWarning("C�digo de invitaci�n {Codigo} no v�lido o expirado", codigo);
                return null;
            }

            var espacio = await _espacioRepository.GetByIdAsync(espacioId, ct);
            if (espacio == null)
            {
                _logger.LogWarning("Espacio {EspacioId} no encontrado para c�digo {Codigo}", espacioId, codigo);
                return null;
            }

            var usuariosEspaciosExistentes = await _usuarioEspacioService.ObtenerPorEspacioAsync(espacioId, ct);
            if (usuariosEspaciosExistentes.Any(ue => ue.UsuarioId == usuarioId))
            {
                _logger.LogWarning("Usuario {UsuarioId} ya est� en el espacio {EspacioId}", usuarioId, espacioId);
                return null;
            }

            var createDto = new CreateUsuarioEspacioDto
            {
                UsuarioId = usuarioId,
                EspacioId = espacioId,
                Ausente = false,
                Karma = 0,
                Rol = "Usuario"
            };

            var usuarioEspacio = await _usuarioEspacioService.CrearUsuarioEspacioAsync(createDto, ct);

            if (usuarioEspacio != null)
            {
                // Actualizar las listas de referencias en Usuario y Espacio
                await _espacioRepository.AddUsuarioEspacioIdAsync(espacioId, usuarioEspacio.Id, ct);
                await _usuarioRepo.AddUsuarioEspacioIdAsync(usuarioId, usuarioEspacio.Id, ct);
                _logger.LogInformation("Usuario {UsuarioId} se uni� al espacio {EspacioId} usando c�digo {Codigo}. UsuarioEspacioId: {UsuarioEspacioId}", usuarioId, espacioId, codigo, usuarioEspacio.Id);
            }

            return usuarioEspacio;
        }

        private string GenerarCodigoAleatorio()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}