using System;
using Mapster;
using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;
using Microsoft.Extensions.Logging;

namespace Convivia.Application.Services
{
    /// <summary>
    /// Servicio de aplicación para gestionar peticiones
    /// Orquesta la lógica de negocio usando el repositorio
    /// SIN acceso directo a Firebase
    /// </summary>
    public class PeticionService
    {
        private readonly IPeticionRepository _repo;
        private readonly ILogger<PeticionService> _logger;

        public PeticionService(IPeticionRepository repo, ILogger<PeticionService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Crear nueva petición
        /// </summary>
        public async Task<PeticionDto> CrearPeticionAsync(CreatePeticionDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Mensaje))
                throw new ArgumentException("El mensaje no puede estar vacío", nameof(dto.Mensaje));
            if (string.IsNullOrWhiteSpace(dto.IdSolicitante))
                throw new ArgumentException("El ID del solicitante no puede estar vacío", nameof(dto.IdSolicitante));
            if (string.IsNullOrWhiteSpace(dto.IdEspacio))
                throw new ArgumentException("El ID del espacio no puede estar vacío", nameof(dto.IdEspacio));

            // Usar Mapster para mapear CreatePeticionDto -> PeticionDto
            var peticion = dto.Adapt<PeticionDto>();
            peticion.Id = Guid.NewGuid().ToString("N");
            peticion.Fecha = DateTime.UtcNow;
            peticion.Estado = "pendiente";

            try
            {
                var id = await _repo.AddAsync(peticion);
                peticion.Id = id;
                return peticion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando petición");
                throw;
            }
        }

        /// <summary>
        /// Obtener petición por ID
        /// </summary>
        public async Task<PeticionDto?> ObtenerPeticionAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                return await _repo.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorId {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Listar todas las peticiones
        /// </summary>
        public async Task<List<PeticionDto>> ListarTodasAsync()
        {
            try
            {
                var entities = await _repo.GetAllAsync();
                return entities.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ListarTodas");
                throw;
            }
        }

        /// <summary>
        /// Obtener peticiones por estado
        /// </summary>
        public async Task<List<PeticionDto>> ListarPorEstadoAsync(string estado)
        {
            if (string.IsNullOrWhiteSpace(estado)) throw new ArgumentNullException(nameof(estado));
            try
            {
                var entities = await _repo.GetByEstadoAsync(estado);
                return entities.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ListarPorEstado {Estado}", estado);
                throw;
            }
        }

        /// <summary>
        /// Cambiar estado de petición (aceptar, rechazar, cancelar)
        /// </summary>
        public async Task<PeticionDto?> CambiarEstadoAsync(string id, string accion)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(accion)) throw new ArgumentNullException(nameof(accion));

            var peticion = await _repo.GetByIdAsync(id);
            if (peticion == null) return null;

            switch (accion.ToLower())
            {
                case "aceptar":
                    if (peticion.Estado != "pendiente")
                        throw new InvalidOperationException($"No se puede aceptar una petición en estado '{peticion.Estado}'.");
                    peticion.Estado = "aceptada";
                    break;
                case "rechazar":
                    if (peticion.Estado != "pendiente")
                        throw new InvalidOperationException($"No se puede rechazar una petición en estado '{peticion.Estado}'.");
                    peticion.Estado = "rechazada";
                    break;
                case "cancelar":
                    if (peticion.Estado != "pendiente")
                        throw new InvalidOperationException($"No se puede cancelar una petición en estado '{peticion.Estado}'.");
                    peticion.Estado = "cancelada";
                    break;
                default:
                    throw new ArgumentException($"Acción '{accion}' no válida. Use: aceptar, rechazar o cancelar.");
            }

            try
            {
                await _repo.UpdateAsync(id, peticion);
                return peticion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error CambiarEstado {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Actualización parcial (PATCH)
        /// </summary>
        public async Task<PeticionDto?> ActualizarParcialAsync(string id, UpdatePeticionDto dto)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var peticion = await _repo.GetByIdAsync(id);
            if (peticion == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.Mensaje))
                peticion.Mensaje = dto.Mensaje;
            if (!string.IsNullOrWhiteSpace(dto.Estado))
                peticion.Estado = dto.Estado;
            if (!string.IsNullOrWhiteSpace(dto.IdSolicitante))
                peticion.IdSolicitante = dto.IdSolicitante;
            if (!string.IsNullOrWhiteSpace(dto.IdEspacio))
                peticion.IdEspacio = dto.IdEspacio;

            try
            {
                await _repo.UpdateAsync(id, peticion);
                return peticion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ActualizarParcial {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Eliminar petición
        /// </summary>
        public async Task<bool> EliminarPeticionAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            try
            {
                await _repo.DeleteAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error EliminarPeticion {Id}", id);
                throw;
            }
        }
    }
}
