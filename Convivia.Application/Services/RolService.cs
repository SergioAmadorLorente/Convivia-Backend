using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Microsoft.Extensions.Logging;
using Convivia.Shared.Repositories;
using Mapster;
using Convivia.Domain.Entities;

namespace Convivia.Application.Services
{
    public class RolService
    {
        private readonly IRolRepository _repo;
        private readonly ILogger<RolService> _logger;
        private static readonly string[] NombresValidos = { "Usuario", "Admin" };

        public RolService(IRolRepository repo, ILogger<RolService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> CrearAsync(CreateRolDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("Nombre requerido");

            // Validar que el nombre sea válido
            if (!EsNombreValido(dto.Nombre))
            {
                throw new ArgumentException($"Nombre '{dto.Nombre}' no válido. Los nombres permitidos son: {string.Join(", ", NombresValidos)}");
            }

            // Crear rol con la configuración apropiada
            Rol rolDomain = new Rol();
            
            if (dto.Nombre.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                rolDomain.SetConfigurarcionAdmin();
            }
            else // Usuario por defecto
            {
                rolDomain.SetConfigurarcionUsuario();
            }

            var rolDto = new RolDto
            {
                Id = Guid.NewGuid().ToString("N"),
                Nombre = rolDomain.Nombre,
                CrearTarea = rolDomain.CrearTarea,
                EliminarTarea = rolDomain.EliminarTarea,
                EditarTarea = rolDomain.EditarTarea,
                AñadirUsuario = rolDomain.AñadirUsuario,
                EliminarUsuario = rolDomain.EliminarUsuario,
                AsignarTarea = rolDomain.AsignarTarea,
                AsignarseTarea = rolDomain.AsignarseTarea
            };

            try
            {
                return await _repo.AddAsync(rolDto, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando rol");
                throw;
            }
        }

        public async Task<RolDto?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                return await _repo.GetByIdAsync(id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorId {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<RolDto>> ObtenerTodosAsync(CancellationToken ct = default)
        {
            try
            {
                return await _repo.GetAllAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerTodos");
                throw;
            }
        }

        public async Task<RolDto?> ObtenerPorNombreAsync(string nombre, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return null;
            
            // Validar que el nombre sea válido
            if (!EsNombreValido(nombre))
            {
                throw new ArgumentException($"Nombre '{nombre}' no válido. Los nombres permitidos son: {string.Join(", ", NombresValidos)}");
            }

            try
            {
                return await _repo.GetByNombreAsync(nombre, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ObtenerPorNombre {Nombre}", nombre);
                throw;
            }
        }

        public async Task<bool> ActualizarAsync(string id, UpdateRolDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await ObtenerPorIdAsync(id, ct);
            if (existing == null) throw new KeyNotFoundException("Rol no encontrado");

            // Validar nombre si se está actualizando
            if (dto.Nombre != null && !EsNombreValido(dto.Nombre))
            {
                throw new ArgumentException($"Nombre '{dto.Nombre}' no válido. Los nombres permitidos son: {string.Join(", ", NombresValidos)}");
            }

            // Si se está cambiando el nombre, aplicar la configuración predefinida
            if (dto.Nombre != null && dto.Nombre != existing.Nombre)
            {
                var rolDomain = new Rol();
                if (dto.Nombre.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    rolDomain.SetConfigurarcionAdmin();
                }
                else
                {
                    rolDomain.SetConfigurarcionUsuario();
                }
                
                existing.Nombre = rolDomain.Nombre;
                existing.CrearTarea = rolDomain.CrearTarea;
                existing.EliminarTarea = rolDomain.EliminarTarea;
                existing.EditarTarea = rolDomain.EditarTarea;
                existing.AñadirUsuario = rolDomain.AñadirUsuario;
                existing.EliminarUsuario = rolDomain.EliminarUsuario;
                existing.AsignarTarea = rolDomain.AsignarTarea;
                existing.AsignarseTarea = rolDomain.AsignarseTarea;
            }
            else
            {
                // Actualizar solo campos no nulos si no se está cambiando el nombre
                if (dto.CrearTarea.HasValue) existing.CrearTarea = dto.CrearTarea.Value;
                if (dto.EliminarTarea.HasValue) existing.EliminarTarea = dto.EliminarTarea.Value;
                if (dto.EditarTarea.HasValue) existing.EditarTarea = dto.EditarTarea.Value;
                if (dto.AñadirUsuario.HasValue) existing.AñadirUsuario = dto.AñadirUsuario.Value;
                if (dto.EliminarUsuario.HasValue) existing.EliminarUsuario = dto.EliminarUsuario.Value;
                if (dto.AsignarTarea.HasValue) existing.AsignarTarea = dto.AsignarTarea.Value;
                if (dto.AsignarseTarea.HasValue) existing.AsignarseTarea = dto.AsignarseTarea.Value;
            }

            try
            {
                await _repo.UpdateAsync(id, existing, ct);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Actualizar {Id}", id);
                throw;
            }
        }

        public async Task<bool> EliminarAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            try
            {
                await _repo.DeleteAsync(id, ct);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error EliminarRol {Id}", id);
                throw;
            }
        }

        private static bool EsNombreValido(string nombre)
        {
            return !string.IsNullOrWhiteSpace(nombre) && 
                   NombresValidos.Contains(nombre, StringComparer.OrdinalIgnoreCase);
        }
    }
}
