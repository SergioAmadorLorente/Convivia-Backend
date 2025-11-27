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
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly IFirebaseService _firebase;
        private readonly ILogger<UsuarioRepository> _logger;
        private const string Collection = "usuarios";

        public UsuarioRepository(IFirebaseService firebase, ILogger<UsuarioRepository> logger)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(UsuarioDto usuario, CancellationToken ct = default)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));
            
            // Convertir UsuarioDto → Usuario (Domain) → UsuarioPersist (Firestore)
            var usuarioDomain = usuario.Adapt<Usuario>();
            var usuarioPersist = usuarioDomain.Adapt<UsuarioPersist>();
            
            if (string.IsNullOrWhiteSpace(usuarioPersist.Id))
            {
                // Si no tiene id, pedimos a Firestore que genere una id y la devolvemos
                var generatedId = await _firebase.AddAsync(Collection, usuarioPersist, ct);
                return generatedId;
            }

            // Si ya tiene id, lo usamos para crear el documento con ese id
            await _firebase.AddAsync(Collection, usuarioPersist.Id, usuarioPersist, ct);
            return usuarioPersist.Id;
        }

        public async Task<UsuarioDto?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                // Obtener UsuarioPersist de Firestore
                var usuarioPersist = await _firebase.GetAsync<UsuarioPersist>(Collection, id, ct);
                if (usuarioPersist == null) return null;
                
                // Convertir UsuarioPersist → Usuario (Domain) → UsuarioDto
                var usuarioDomain = usuarioPersist.Adapt<Usuario>();
                return usuarioDomain.Adapt<UsuarioDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByIdAsync {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<UsuarioDto>> GetByFullNameInvitadoAsync(string Nombre, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(Nombre)) return Array.Empty<UsuarioDto>();
            try
            {
                // Consultar UsuarioPersist desde Firestore
                var list = await _firebase.QueryAsync<UsuarioPersist>(Collection, nameof(UsuarioPersist.Nombre), Nombre, ct);
                if (list == null || !list.Any()) return new List<UsuarioDto>();
                
                // Convertir UsuarioPersist → Usuario (Domain) → UsuarioDto
                return list.Select(up => up.Adapt<Usuario>().Adapt<UsuarioDto>()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByUsuarioInvitadoAsync {Usuario}", Nombre);
                throw;
            }
        }

        public async Task UpdateAsync(string id, UsuarioDto usuario, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));

            try
            {
                // Convertir UsuarioDto → Usuario (Domain) → UsuarioPersist
                var usuarioDomain = usuario.Adapt<Usuario>();
                var usuarioPersist = usuarioDomain.Adapt<UsuarioPersist>();
                
                await _firebase.UpdateAsync(Collection, id, usuarioPersist, ct);
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
