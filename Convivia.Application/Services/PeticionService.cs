using Mapster;
using Convivia.Application.DTOs;
using Convivia.Application.Interfaces;
using Convivia.Domain.Models;

namespace Convivia.Application.Services
{
    /// <summary>
    /// Servicio de aplicación para gestionar peticiones
    /// Orquesta la lógica de negocio usando el repositorio
    /// SIN acceso directo a Firebase
    /// </summary>
    public class PeticionService
    {
        private readonly IPeticionRepository _peticionRepository;

        public PeticionService(IPeticionRepository peticionRepository)
        {
            _peticionRepository = peticionRepository ?? throw new ArgumentNullException(nameof(peticionRepository));
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

            Console.WriteLine($"[PeticionService] Creando petición: {dto.Mensaje}");
            Console.WriteLine($"[PeticionService] ID proporcionado: {dto.Id ?? "(null - se generará GUID)"}");
            Console.WriteLine($"[PeticionService] IdSolicitante: {dto.IdSolicitante}");
            Console.WriteLine($"[PeticionService] IdEspacio: {dto.IdEspacio}");

            // 1. Generar ID si no se proporcionó
            string idFinal = string.IsNullOrWhiteSpace(dto.Id) 
                ? Guid.NewGuid().ToString("N") 
                : dto.Id;

            Console.WriteLine($"[PeticionService] ID final a usar: {idFinal}");

            // 2. Crear entidad de dominio con ID explícito
            var entity = new Peticion(idFinal, dto.Mensaje, dto.IdSolicitante, dto.IdEspacio);

            Console.WriteLine($"[PeticionService] Entidad creada con ID: {entity.Id}");

            // 3. Guardar usando el repositorio
            await _peticionRepository.AddAsync(entity);
            
            Console.WriteLine($"[PeticionService] ? Petición guardada exitosamente con ID: {entity.Id}");

            // 4. Devolver DTO
            return entity.Adapt<PeticionDto>();
        }

        /// <summary>
        /// Obtener petición por ID
        /// </summary>
        public async Task<PeticionDto?> ObtenerPeticionAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            Console.WriteLine($"[PeticionService] Obteniendo petición con ID: {id}");

            var entity = await _peticionRepository.GetByIdAsync(id);
            if (entity == null)
            {
                Console.WriteLine($"[PeticionService] No se encontró petición con ID: {id}");
                return null;
            }

            Console.WriteLine($"[PeticionService] Petición encontrada: {entity.Mensaje}");
            return entity.Adapt<PeticionDto>();
        }

        /// <summary>
        /// Listar todas las peticiones
        /// </summary>
        public async Task<List<PeticionDto>> ListarTodasAsync()
        {
            Console.WriteLine($"[PeticionService] Obteniendo todas las peticiones");
            
            var entities = await _peticionRepository.GetAllAsync();
            
            Console.WriteLine($"[PeticionService] Se obtuvieron {entities.Count} peticiones");
            
            return entities.Adapt<List<PeticionDto>>();
        }

        /// <summary>
        /// Obtener peticiones por estado
        /// </summary>
        public async Task<List<PeticionDto>> ListarPorEstadoAsync(string estado)
        {
            if (string.IsNullOrWhiteSpace(estado)) throw new ArgumentNullException(nameof(estado));

            Console.WriteLine($"[PeticionService] Filtrando peticiones por estado: {estado}");

            var entities = await _peticionRepository.GetByEstadoAsync(estado);
            
            Console.WriteLine($"[PeticionService] Se encontraron {entities.Count} peticiones con estado '{estado}'");

            return entities.Adapt<List<PeticionDto>>();
        }

        /// <summary>
        /// Cambiar estado de petición (aceptar, rechazar, cancelar)
        /// </summary>
        public async Task<PeticionDto?> CambiarEstadoAsync(string id, string accion)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(accion)) throw new ArgumentNullException(nameof(accion));

            Console.WriteLine($"[PeticionService] Cambiando estado de petición {id} a acción: {accion}");

            // 1. Obtener entidad desde el repositorio
            var entity = await _peticionRepository.GetByIdAsync(id);
            if (entity == null) return null;

            // 2. Aplicar acción (método de negocio)
            switch (accion.ToLower())
            {
                case "aceptar":
                    entity.Aceptar();
                    break;
                case "rechazar":
                    entity.Rechazar();
                    break;
                case "cancelar":
                    entity.Cancelar();
                    break;
                default:
                    throw new ArgumentException($"Acción '{accion}' no válida. Use: aceptar, rechazar o cancelar.");
            }

            Console.WriteLine($"[PeticionService] Nuevo estado: {entity.Estado}");

            // 3. Actualizar usando el repositorio
            await _peticionRepository.UpdateAsync(entity);

            // 4. Devolver DTO actualizado
            return entity.Adapt<PeticionDto>();
        }

        /// <summary>
        /// Actualización parcial (PATCH)
        /// </summary>
        public async Task<PeticionDto?> ActualizarParcialAsync(string id, UpdatePeticionDto dto)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            Console.WriteLine($"[PeticionService] Actualizando parcialmente petición: {id}");

            // 1. Obtener entidad desde el repositorio
            var entity = await _peticionRepository.GetByIdAsync(id);
            if (entity == null) return null;

            // 2. Aplicar cambios (esto requeriría métodos en la entidad)
            // Por ahora, obtenemos y re-construimos con los cambios
            var mensajeActualizado = dto.Mensaje ?? entity.Mensaje;
            var estadoActualizado = dto.Estado ?? entity.Estado;
            var idSolicitanteActualizado = dto.IdSolicitante ?? entity.IdSolicitante;
            var idEspacioActualizado = dto.IdEspacio ?? entity.IdEspacio;

            var entityActualizada = Peticion.Reconstruir(
                entity.Id,
                mensajeActualizado,
                entity.Fecha,
                estadoActualizado,
                idSolicitanteActualizado,
                idEspacioActualizado
            );

            // 3. Guardar cambios
            await _peticionRepository.UpdateAsync(entityActualizada);

            Console.WriteLine($"[PeticionService] Petición actualizada: {entityActualizada.Mensaje}, Estado: {entityActualizada.Estado}");

            // 4. Devolver DTO
            return entityActualizada.Adapt<PeticionDto>();
        }

        /// <summary>
        /// Eliminar petición
        /// </summary>
        public async Task<bool> EliminarPeticionAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            Console.WriteLine($"[PeticionService] Eliminando petición: {id}");

            var deleted = await _peticionRepository.DeleteAsync(id);
            
            if (deleted)
            {
                Console.WriteLine($"[PeticionService] Petición eliminada: {id}");
            }
            else
            {
                Console.WriteLine($"[PeticionService] No se encontró petición para eliminar: {id}");
            }

            return deleted;
        }
    }
}
