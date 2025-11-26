using Convivia.Application.DTOs;
using Convivia.Shared.Repositories;
using Convivia.Shared.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Shared.Repositories;

namespace Convivia.Infrastructure.Repositories
{
    public class EspacioRepository : IEspacioRepository
    {
        private readonly IFirebaseService _firebase;
        private readonly ILogger<EspacioRepository> _logger;
        private const string Collection = "espacios";

        public EspacioRepository(IFirebaseService firebase, ILogger<EspacioRepository> logger)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AddAsync(EspacioDto espacio, CancellationToken ct = default)
        {
            if (espacio == null) throw new ArgumentNullException(nameof(espacio));
            if (string.IsNullOrWhiteSpace(espacio.Id))
            {
                // Si no tiene id, pedimos a Firestore que genere una id y la devolvemos
                var generatedId = await _firebase.AddAsync(Collection, espacio, ct);
                return generatedId;
            }

            // Si ya tiene id, lo usamos para crear el documento con ese id
            await _firebase.AddAsync(Collection, espacio.Id, espacio, ct);
            return espacio.Id;
        }

        public async Task<EspacioDto?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                return await _firebase.GetAsync<EspacioDto>(Collection, id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByIdAsync {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<EspacioDto>> GetByDireccionAsync(string direccion, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(direccion)) return Array.Empty<EspacioDto>();
            try
            {
                var list = await _firebase.QueryAsync<EspacioDto>(Collection, nameof(EspacioDto.Direccion), direccion, ct);
                return list ?? new List<EspacioDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByUsuarioInvitadoAsync {Usuario}", direccion);
                throw;
            }
        }

        public async Task UpdateAsync(string id, EspacioDto espacio, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id requerido", nameof(id));
            if (espacio == null) throw new ArgumentNullException(nameof(espacio));

            try
            {
                await _firebase.UpdateAsync(Collection, id, espacio, ct);
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