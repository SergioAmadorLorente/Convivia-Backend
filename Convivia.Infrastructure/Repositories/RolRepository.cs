using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Convivia.Infrastructure.Models;
using Convivia.Domain.Entities;
using Microsoft.Extensions.Logging;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Convivia.Infrastructure.Repositories
{
    public class RolRepository : IRolRepository
    {
        private readonly IFirebaseService _firebase;
        private readonly ILogger<RolRepository> _logger;
        private const string Collection = "roles";

        public RolRepository(IFirebaseService firebase, ILogger<RolRepository> logger)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(RolDto rol, CancellationToken ct = default)
        {
            if (rol == null) throw new ArgumentNullException(nameof(rol));
            
            // Convertir RolDto ? Rol (Domain) ? FireStoreRol (Firestore)
            var rolDomain = rol.Adapt<Rol>();
            var rolPersist = rolDomain.Adapt<FireStoreRol>();
            
            if (string.IsNullOrWhiteSpace(rolPersist.Id))
            {
                // Si no tiene id, pedimos a Firestore que genere una id y la devolvemos
                var generatedId = await _firebase.AddAsync(Collection, rolPersist, ct);
                return generatedId;
            }

            // Si ya tiene id, lo usamos para crear el documento con ese id
            await _firebase.AddAsync(Collection, rolPersist.Id, rolPersist, ct);
            return rolPersist.Id;
        }

        public async Task<RolDto?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                // Obtener FireStoreRol de Firestore
                var rolPersist = await _firebase.GetAsync<FireStoreRol>(Collection, id, ct);
                if (rolPersist == null) return null;
                
                // Convertir FireStoreRol ? Rol (Domain) ? RolDto
                var rolDomain = rolPersist.Adapt<Rol>();
                return rolDomain.Adapt<RolDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByIdAsync {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<RolDto>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                // Obtener todos los FireStoreRol desde Firestore
                var list = await _firebase.GetAllAsync<FireStoreRol>(Collection, ct);
                if (list == null || !list.Any()) return new List<RolDto>();
                
                // Convertir FireStoreRol ? Rol (Domain) ? RolDto
                return list.Select(rp => rp.Adapt<Rol>().Adapt<RolDto>()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetAllAsync");
                throw;
            }
        }

        public async Task<RolDto?> GetByNombreAsync(string nombre, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return null;
            try
            {
                // Consultar FireStoreRol desde Firestore por nombre
                var list = await _firebase.QueryAsync<FireStoreRol>(Collection, nameof(FireStoreRol.Nombre), nombre, ct);
                if (list == null || !list.Any()) return null;
                
                var rolPersist = list.FirstOrDefault();
                if (rolPersist == null) return null;
                
                // Convertir FireStoreRol ? Rol (Domain) ? RolDto
                var rolDomain = rolPersist.Adapt<Rol>();
                return rolDomain.Adapt<RolDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByNombreAsync {Nombre}", nombre);
                throw;
            }
        }

        public async Task UpdateAsync(string id, RolDto rol, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));
            if (rol == null) throw new ArgumentNullException(nameof(rol));

            try
            {
                // Convertir RolDto ? Rol (Domain) ? FireStoreRol
                var rolDomain = rol.Adapt<Rol>();
                var rolPersist = rolDomain.Adapt<FireStoreRol>();
                
                await _firebase.UpdateAsync(Collection, id, rolPersist, ct);
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
