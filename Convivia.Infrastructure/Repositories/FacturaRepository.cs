using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.Services;
using Convivia.Infrastructure.Models;
using Convivia.Domain.Entities;
using Microsoft.Extensions.Logging;
using Mapster;
using Convivia.Application.Repositories;

namespace Convivia.Infrastructure.Repositories
{
    public class FacturaRepository : IFacturaRepository
    {
        private readonly IFirebaseService _firebase;
        private readonly ILogger<FacturaRepository> _logger;
        private const string ParentCollection = "espacios";
        private const string SubCollection = "facturas";

        public FacturaRepository(IFirebaseService firebase, ILogger<FacturaRepository> logger)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string GetCollectionPath(string espacioId)
        {
            if (string.IsNullOrWhiteSpace(espacioId))
                throw new ArgumentException("espacioId es requerido", nameof(espacioId));
            return $"{ParentCollection}/{espacioId}/{SubCollection}";
        }

        public async Task<string> AddAsync(string espacioId, Factura factura, CancellationToken ct = default)
        {
            if (factura == null) throw new ArgumentNullException(nameof(factura));
            var persist = factura.Adapt<FireStoreFactura>();
            var collectionPath = GetCollectionPath(espacioId);
            return await _firebase.AddAsync(collectionPath, persist, ct);
        }

        public async Task<Factura?> GetByIdAsync(string espacioId, string id, CancellationToken ct = default)
        {
            var collectionPath = GetCollectionPath(espacioId);
            var persist = await _firebase.GetAsync<FireStoreFactura>(collectionPath, id, ct);
            if (persist == null) return null;
            return persist.Adapt<Factura>();
        }

        public async Task<IEnumerable<Factura>> GetAllAsync(string espacioId, CancellationToken ct = default)
        {
            var collectionPath = GetCollectionPath(espacioId);
            var list = await _firebase.GetAllAsync<FireStoreFactura>(collectionPath, ct);
            return list == null ? Array.Empty<Factura>() : list.Adapt<List<Factura>>();
        }

        public async Task UpdateAsync(string espacioId, string id, Factura factura, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (factura == null) throw new ArgumentNullException(nameof(factura));

            var persist = factura.Adapt<FireStoreFactura>();
            var collectionPath = GetCollectionPath(espacioId);
            await _firebase.UpdateAsync(collectionPath, id, persist, ct);
        }

        public async Task UpdateAsync(string espacioId, string id, Factura factura, bool merge, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (factura == null) throw new ArgumentNullException(nameof(factura));

            var persist = factura.Adapt<FireStoreFactura>();
            var collectionPath = GetCollectionPath(espacioId);
            await _firebase.UpdateAsync(collectionPath, id, persist, merge, ct);
        }

        public async Task UpdateAsync(string espacioId, string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default)
        {
            var collectionPath = GetCollectionPath(espacioId);
            await _firebase.UpdateAsync(collectionPath, id, updates, useSetMerge, ct);
        }

        public async Task DeleteAsync(string espacioId, string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            var collectionPath = GetCollectionPath(espacioId);
            await _firebase.DeleteAsync(collectionPath, id, ct);
        }

        public async Task<IEnumerable<Factura>> QueryByFieldAsync(string espacioId, string field, object value, CancellationToken ct = default)
        {
            var collectionPath = GetCollectionPath(espacioId);
            var list = await _firebase.QueryAsync<FireStoreFactura>(collectionPath, field, value, ct);
            return list == null ? Array.Empty<Factura>() : list.Adapt<List<Factura>>();
        }

        // Nueva implementación: consulta eficiente que devuelve si existe al menos una asociación
        public async Task<bool> ExistsByUsuarioEspacioIdAsync(string usuarioEspacioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioEspacioId)) return false;
            try
            {
                // Buscar en todas las subcolecciones de facturas de todos los espacios
                // Nota: esto puede ser ineficiente, considerar reestructurar si es necesario
                var list = await _firebase.QueryAsync<FireStoreFactura>(ParentCollection, "FacturaRef", usuarioEspacioId, ct);
                return list != null && list.Count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ExistsByUsuarioEspacioId {usuarioEspacioId}", usuarioEspacioId);
                throw;
            }
        }
    }
}
