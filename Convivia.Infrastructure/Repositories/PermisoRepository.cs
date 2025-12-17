using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Convivia.Infrastructure.Models;
using Convivia.Domain.Entities;
using Microsoft.Extensions.Logging;
using Mapster;

namespace Convivia.Infrastructure.Repositories
{
    public class PermisoRepository : IPermisoRepository
    {
        private readonly IFirebaseService _firebase;
        private readonly ILogger<PermisoRepository> _logger;
        private const string Collection = "permisos";

        public PermisoRepository(IFirebaseService firebase, ILogger<PermisoRepository> logger)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(PermisoDto permiso, CancellationToken ct = default)
        {
            if (permiso == null) throw new ArgumentNullException(nameof(permiso));
            
            // Convertir PermisoDto ? Permiso (Domain) ? FireStorePermiso (Firestore)
            var permisoDomain = permiso.Adapt<Permiso>();
            var permisoPersist = permisoDomain.Adapt<FireStorePermiso>();
            
            if (string.IsNullOrWhiteSpace(permisoPersist.Id))
            {
                // Si no tiene id, pedimos a Firestore que genere una id y la devolvemos
                var generatedId = await _firebase.AddAsync(Collection, permisoPersist, ct);
                return generatedId;
            }

            // Si ya tiene id, lo usamos para crear el documento con ese id
            await _firebase.AddAsync(Collection, permisoPersist.Id, permisoPersist, ct);
            return permisoPersist.Id;
        }

        public async Task<PermisoDto?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                // Obtener FireStorePermiso de Firestore
                var permisoPersist = await _firebase.GetAsync<FireStorePermiso>(Collection, id, ct);
                if (permisoPersist == null) return null;
                
                // Convertir FireStorePermiso ? Permiso (Domain) ? PermisoDto
                var permisoDomain = permisoPersist.Adapt<Permiso>();
                return permisoDomain.Adapt<PermisoDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByIdAsync {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<PermisoDto>> GetByRolAsync(string rol, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(rol)) return Array.Empty<PermisoDto>();
            try
            {
                // Consultar FireStorePermiso desde Firestore
                var list = await _firebase.QueryAsync<FireStorePermiso>(Collection, nameof(FireStorePermiso.Rol), rol, ct);
                if (list == null || !list.Any()) return new List<PermisoDto>();
                
                // Convertir FireStorePermiso ? Permiso (Domain) ? PermisoDto
                return list.Select(pp => pp.Adapt<Permiso>().Adapt<PermisoDto>()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByRolAsync {Rol}", rol);
                throw;
            }
        }

        public async Task<IEnumerable<PermisoDto>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                // Obtener todos los permisos desde Firestore
                var list = await _firebase.GetAllAsync<FireStorePermiso>(Collection, ct);
                if (list == null || !list.Any()) return new List<PermisoDto>();
                
                // Convertir FireStorePermiso ? Permiso (Domain) ? PermisoDto
                return list.Select(pp => pp.Adapt<Permiso>().Adapt<PermisoDto>()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetAllAsync");
                throw;
            }
        }

        public async Task UpdateAsync(string id, PermisoDto permiso, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));
            if (permiso == null) throw new ArgumentNullException(nameof(permiso));

            try
            {
                // Convertir PermisoDto ? Permiso (Domain) ? FireStorePermiso
                var permisoDomain = permiso.Adapt<Permiso>();
                var permisoPersist = permisoDomain.Adapt<FireStorePermiso>();
                
                await _firebase.UpdateAsync(Collection, id, permisoPersist, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error UpdateAsync {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));
            try
            {
                await _firebase.DeleteAsync(Collection, id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error DeleteAsync {Id}", id);
                throw;
            }
        }
    }
}
