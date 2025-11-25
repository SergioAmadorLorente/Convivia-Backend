using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if (string.IsNullOrWhiteSpace(usuario.Id))
            {
                // Si no tiene id, pedimos a Firestore que genere una id y la devolvemos
                var generatedId = await _firebase.AddAsync(Collection, usuario, ct);
                return generatedId;
            }

            // Si ya tiene id, lo usamos para crear el documento con ese id
            await _firebase.AddAsync(Collection, usuario.Id, usuario, ct);
            return usuario.Id;
        }

        public async Task<UsuarioDto?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                return await _firebase.GetAsync<UsuarioDto>(Collection, id, ct);
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
                var list = await _firebase.QueryAsync<UsuarioDto>(Collection, nameof(UsuarioDto.Nombre), Nombre, ct);
                return list ?? new List<UsuarioDto>();
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
                await _firebase.UpdateAsync(Collection, id, usuario, ct);
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
