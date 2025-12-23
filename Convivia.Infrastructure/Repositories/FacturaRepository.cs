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
    public class FacturaRepository : Repository<FireStoreFactura>, IFacturaRepository
    {
        private readonly ILogger<FacturaRepository> _logger;
        private const string Collection = "facturas";

        public FacturaRepository(IFirebaseService firebase, ILogger<FacturaRepository> logger, ILoggerFactory loggerFactory)
            : base(firebase, loggerFactory.CreateLogger<Repository<FireStoreFactura>>(), Collection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(Factura factura, CancellationToken ct = default)
        {
            if (factura == null) throw new ArgumentNullException(nameof(factura));
            var persist = factura.Adapt<FireStoreFactura>();
            return await base.AddAsync(persist,persist.Id, ct);
        }

        public async Task<Factura?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var persist = await base.GetByIdAsync(id, ct);
            if (persist == null) return null;
            return persist.Adapt<Factura>();
        }

        public async Task<IEnumerable<Factura>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await base.GetAllAsync(ct);
            return list == null ? Array.Empty<Factura>() : list.Adapt<List<Factura>>();
        }

        public async Task UpdateAsync(string id, Factura factura, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (factura == null) throw new ArgumentNullException(nameof(factura));

            var persist = factura.Adapt<FireStoreFactura>();
            await base.UpdateAsync(id, persist, ct);
        }

        public async Task UpdateAsync(string id, Factura factura, bool merge, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (factura == null) throw new ArgumentNullException(nameof(factura));

            var persist = factura.Adapt<FireStoreFactura>();
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

        public async Task<IEnumerable<Factura>> QueryByFieldAsync(string field, object value, CancellationToken ct = default)
        {
            var list = await _firebase.QueryAsync<FireStoreFactura>(Collection, field, value, ct);
            return list == null ? Array.Empty<Factura>() : list.Adapt<List<Factura>>();
        }
    }
}
