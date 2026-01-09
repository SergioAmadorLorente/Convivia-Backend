using Convivia.Domain.Entities;
using Convivia.Infrastructure.Models;
using Convivia.Shared.DTOs;
using Convivia.Application.Repositories;
using Convivia.Shared.Services;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Convivia.Infrastructure.Repositories
{
    public class UsuarioRepository : Repository<FireStoreUsuario>, IUsuarioRepository
    {
        private readonly ILogger<UsuarioRepository> _logger;
        private const string Collection = "usuarios";

        public UsuarioRepository(IFirebaseService firebase, ILogger<UsuarioRepository> logger, ILoggerFactory loggerFactory)
            : base(firebase, loggerFactory.CreateLogger<Repository<FireStoreUsuario>>(), Collection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(Usuario usuario, CancellationToken ct = default)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));
            var persist = usuario.Adapt<FireStoreUsuario>();
            return await base.AddAsync(persist,persist.Id, ct);
        }

        public async Task<Usuario?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var persist = await base.GetByIdAsync(id, ct);
            if (persist == null) return null;
            return persist.Adapt<Usuario>();
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await base.GetAllAsync(ct);
            return list == null ? Array.Empty<Usuario>() : list.Adapt<List<Usuario>>();
        }

        public async Task<UsuarioDto?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            try
            {
                _logger.LogInformation("GetByEmailAsync llamado para email: {Email}", email);
                var list = await _firebase.QueryAsync<FireStoreUsuario>(Collection, "Email", email, ct);
                
                if (list == null || !list.Any())
                {
                    _logger.LogInformation("No se encontró usuario con email: {Email}", email);
                    return null;
                }

                var usuarioPersist = list.FirstOrDefault();
                if (usuarioPersist == null) return null;

                // Convertir FireStoreUsuario → Usuario (Domain) → UsuarioDto
                var usuarioDomain = usuarioPersist.Adapt<Usuario>();
                var usuarioDto = usuarioDomain.Adapt<UsuarioDto>();
                
                _logger.LogInformation("Usuario encontrado con email: {Email}, Id: {Id}", email, usuarioDto.Id);
                return usuarioDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByEmailAsync {Email}", email);
                throw;
            }
        }
        
        public async Task UpdateAsync(string id, Usuario usuario, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));

            var persist = usuario.Adapt<FireStoreUsuario>();
            await base.UpdateAsync(id, persist, ct);
        }


        public async Task UpdateAsync(string id, Usuario usuario, bool merge, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));

            var persist = usuario.Adapt<FireStoreUsuario>();
            await base.UpdateAsync(id, persist, merge, ct);
        }

        public async Task UpdateAsync(string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default)
        {
            await base.UpdateAsync(id, updates, useSetMerge, ct);
        }


        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            await base.DeleteAsync(id, ct);
        }

        public async Task AddUsuarioEspacioIdAsync(string usuarioId, string usuarioEspacioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioId)) throw new ArgumentException("usuarioId requerido", nameof(usuarioId));
            if (string.IsNullOrWhiteSpace(usuarioEspacioId)) throw new ArgumentException("usuarioEspacioId requerido", nameof(usuarioEspacioId));

            try
            {
                var usuario = await _firebase.GetAsync<FireStoreUsuario>(Collection, usuarioId, ct);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no encontrado", usuarioId);
                    return;
                }

                if (!usuario.UsuarioEspaciosIds.Contains(usuarioEspacioId))
                {
                    usuario.UsuarioEspaciosIds.Add(usuarioEspacioId);
                    await _firebase.UpdateAsync(Collection, usuarioId, usuario, merge: true, ct);
                    _logger.LogInformation("UsuarioEspacioId {UsuarioEspacioId} agregado a Usuario {UsuarioId}", usuarioEspacioId, usuarioId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error AddUsuarioEspacioIdAsync {UsuarioId}, {UsuarioEspacioId}", usuarioId, usuarioEspacioId);
                throw;
            }
        }
    }
}
