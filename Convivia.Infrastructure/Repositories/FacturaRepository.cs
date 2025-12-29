using Convivia.Shared.DTOs;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Convivia.Infrastructure.Models;
using Convivia.Domain.Entities;
using Microsoft.Extensions.Logging;
using Mapster;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Domain.Repositories;

namespace Convivia.Infrastructure.Repositories
{
    public class FacturaRepository : IFacturaRepository
    {
        private readonly IFirebaseService _firebase;
        private readonly ILogger<FacturaRepository> _logger;
        private const string Collection = "facturas";

        public FacturaRepository(IFirebaseService firebase, ILogger<FacturaRepository> logger)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(CreateFacturaDto factura, CancellationToken ct = default)
        {
            if (factura == null) throw new ArgumentNullException(nameof(factura));

            // Map CreateFacturaDto → Domain.Factura → FireStoreFactura
            var facturaDomain = factura.Adapt<Factura>();
            var facturaPersist = facturaDomain.Adapt<FireStoreFactura>();

            // Si no tiene id, Firestore lo genera
            var generatedId = await _firebase.AddAsync(Collection, facturaPersist, ct);
            return generatedId;
        }

        public async Task<FacturaDto?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                var facturaPersist = await _firebase.GetAsync<FireStoreFactura>(Collection, id, ct);
                if (facturaPersist == null) return null;

                var facturaDomain = facturaPersist.Adapt<Factura>();
                return facturaDomain.Adapt<FacturaDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByIdAsync {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<FacturaDto>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await _firebase.GetAllAsync<FireStoreFactura>(Collection, ct);
            var domainList = list.Adapt<List<Factura>>();
            return domainList.Adapt<List<FacturaDto>>();
        }

        public async Task UpdateAsync(string id, UpdateFacturaDto factura, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (factura == null) throw new ArgumentNullException(nameof(factura));

            var facturaDomain = factura.Adapt<Factura>();
            var facturaPersist = facturaDomain.Adapt<FireStoreFactura>();

            await _firebase.UpdateAsync(Collection, id, facturaPersist, ct);
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            await _firebase.DeleteAsync(Collection, id, ct);
        }

        public async Task<IEnumerable<FacturaDto>> QueryByFieldAsync(string field, object value, CancellationToken ct = default)
        {
            var list = await _firebase.QueryAsync<FireStoreFactura>(Collection, field, value, ct);
            var domainList = list.Adapt<List<Factura>>();
            return domainList.Adapt<List<FacturaDto>>();
        }

        public async Task<IEnumerable<FacturaDto>> GetByUsuarioEspacioIdAsync(
    string usuarioEspacioid,
    CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioEspacioid))
                throw new ArgumentNullException(nameof(usuarioEspacioid));

            // 1) Consultar Firestore por facturas donde RepartoKeys contiene el usuario
            var facturasFirestore = await _firebase.QueryArrayContainsAsync<FireStoreFactura>(
                Collection,
                "RepartoKeys",
                usuarioEspacioid
            );

            // 2) Mapear a DTOs
            var lista = new List<FacturaDto>();

            foreach (var dtoFirestore in facturasFirestore)
            {
                if (dtoFirestore == null)
                    continue;

                var facturaDto = dtoFirestore.Adapt<FacturaDto>();
                lista.Add(facturaDto);
            }

            return lista;
        }
        // Nueva implementación: consulta eficiente que devuelve si existe al menos una asociación
        public async Task<bool> ExistsByUsuarioEspacioIdAsync(string usuarioEspacioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioEspacioId)) return false;
            try
            {
                // Usar el nombre real del campo en Firestore: "EspacioRef"
                var list = await _firebase.QueryAsync<FireStoreUsuarioEspacio>(Collection, "FacturaRef", usuarioEspacioId, ct);
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
