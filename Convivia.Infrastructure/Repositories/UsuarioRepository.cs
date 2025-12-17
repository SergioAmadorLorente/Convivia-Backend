using Convivia.Shared.DTOs;
using Convivia.Application.Repositories;
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
            return await base.AddAsync(persist, ct);
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
    }
}
