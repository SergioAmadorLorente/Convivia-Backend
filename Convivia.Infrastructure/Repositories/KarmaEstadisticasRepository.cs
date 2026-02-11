using Convivia.Domain.Entities;
using Convivia.Application.Repositories;
using Convivia.Infrastructure.Models;
using Convivia.Shared.Services;
using Microsoft.Extensions.Logging;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Infrastructure.Repositories
{
    public class KarmaEstadisticasRepository : Repository<FirestoreKarmaEstadisticas>, IKarmaEstadisticasRepository
    {
        private readonly ILogger<KarmaEstadisticasRepository> _logger;
        private const string ParentCollection = "espacios";
        private const string SubCollection = "karma";

        public KarmaEstadisticasRepository(
            IFirebaseService firebase,
            ILogger<KarmaEstadisticasRepository> logger,
            ILoggerFactory loggerFactory)
            : base(firebase, loggerFactory.CreateLogger<Repository<FirestoreKarmaEstadisticas>>(), SubCollection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string GetKarmaCollectionPath(string espacioId) => $"{ParentCollection}/{espacioId}/{SubCollection}";

        public async Task<string> AddAsync(String espacioId, KarmaEstadisticas entity, CancellationToken ct = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            /*var usuarioEspacio = await GetUsuarioEspacioAsync(entity.UsuarioEspacioId);
            if (usuarioEspacio == null)
                throw new InvalidOperationException($"No se encontró UsuarioEspacio con ID {entity.UsuarioEspacioId}");*/

            var collectionPath = GetKarmaCollectionPath(espacioId);
            var persist = entity.Adapt<FirestoreKarmaEstadisticas>();
            
            await _firebase.AddAsync(collectionPath, entity.Id, persist, ct);
            return entity.Id;
        }

        public async Task<KarmaEstadisticas?> GetByUsuarioEspacioIdAsync(string espacioId, string usuarioEspacioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId) || string.IsNullOrWhiteSpace(usuarioEspacioId))
                return null;

            try
            {
                var collectionPath = GetKarmaCollectionPath(espacioId);
                var persist = await _firebase.GetAsync<FirestoreKarmaEstadisticas>(collectionPath, usuarioEspacioId, ct);
                
                if (persist == null)
                    return null;

                var estadisticas = persist.Adapt<KarmaEstadisticas>();
                
                // Verificar si necesita reset automático por cambio de período
                bool needsUpdate = false;
                int currentWeek = GetCurrentWeek();
                int currentMonth = GetCurrentMonth();

                // Reset semanal si cambió la semana
                if (!estadisticas.UltimaSemana.HasValue || estadisticas.UltimaSemana.Value < currentWeek)
                {
                    estadisticas.KarmaSemanal = 0;
                    estadisticas.UltimaSemana = currentWeek;
                    needsUpdate = true;
                    _logger.LogInformation("Karma semanal reseteado automáticamente para {UsuarioEspacioId} (semana {Week})", 
                        usuarioEspacioId, currentWeek);
                }

                // Reset mensual si cambió el mes
                if (!estadisticas.UltimoMes.HasValue || estadisticas.UltimoMes.Value < currentMonth)
                {
                    estadisticas.KarmaMensual = 0;
                    estadisticas.UltimoMes = currentMonth;
                    needsUpdate = true;
                    _logger.LogInformation("Karma mensual reseteado automáticamente para {UsuarioEspacioId} (mes {Month})", 
                        usuarioEspacioId, currentMonth);
                }

                // Si hubo cambios, actualizar en Firestore
                if (needsUpdate)
                {
                    estadisticas.UltimaActualizacion = DateTime.UtcNow;
                    await UpdateAsync(espacioId, estadisticas.Id, estadisticas, ct);
                }

                return estadisticas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo karma de {UsuarioEspacioId} en {EspacioId}", usuarioEspacioId, espacioId);
                throw;
            }
        }

        public async Task<KarmaEstadisticas> AddKarmaAsync(string espacioId, string usuarioEspacioId, int karmaAmount, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId))
                throw new ArgumentNullException(nameof(espacioId));
            
            if (string.IsNullOrWhiteSpace(usuarioEspacioId))
                throw new ArgumentNullException(nameof(usuarioEspacioId));

            if (karmaAmount <= 0)
                throw new ArgumentException("karmaAmount debe ser mayor a 0", nameof(karmaAmount));

            var estadisticas = await GetByUsuarioEspacioIdAsync(espacioId, usuarioEspacioId, ct);
            
            if (estadisticas == null)
                throw new InvalidOperationException($"No existen estadísticas para UsuarioEspacio {usuarioEspacioId}. Deben crearse primero.");

            // El reset automático ya se aplicó en GetByUsuarioEspacioIdAsync si era necesario
            
            // Sumar karma
            estadisticas.KarmaTotal += karmaAmount;
            estadisticas.KarmaSemanal += karmaAmount;
            estadisticas.KarmaMensual += karmaAmount;
            estadisticas.UltimaActualizacion = DateTime.UtcNow;

            await UpdateAsync(espacioId, estadisticas.Id, estadisticas, ct);

            _logger.LogDebug("Karma +{Amount} ? Usuario {Id}: Total={Total}, Semanal={Semanal}, Mensual={Mensual}", 
                karmaAmount, usuarioEspacioId, estadisticas.KarmaTotal, estadisticas.KarmaSemanal, estadisticas.KarmaMensual);

            return estadisticas;
        }

        public async Task<KarmaEstadisticas> SubtractKarmaAsync(string espacioId, string usuarioEspacioId, int karmaAmount, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId))
                throw new ArgumentNullException(nameof(espacioId));
            
            if (string.IsNullOrWhiteSpace(usuarioEspacioId))
                throw new ArgumentNullException(nameof(usuarioEspacioId));

            if (karmaAmount <= 0)
                throw new ArgumentException("karmaAmount debe ser mayor a 0", nameof(karmaAmount));

            var estadisticas = await GetByUsuarioEspacioIdAsync(espacioId, usuarioEspacioId, ct);
            
            if (estadisticas == null)
                throw new InvalidOperationException($"No existen estadísticas para UsuarioEspacio {usuarioEspacioId}.");

            // El reset automático ya se aplicó en GetByUsuarioEspacioIdAsync si era necesario
            
            // Restar karma (sin permitir valores negativos)
            estadisticas.KarmaTotal = Math.Max(0, estadisticas.KarmaTotal - karmaAmount);
            estadisticas.KarmaSemanal = Math.Max(0, estadisticas.KarmaSemanal - karmaAmount);
            estadisticas.KarmaMensual = Math.Max(0, estadisticas.KarmaMensual - karmaAmount);
            estadisticas.UltimaActualizacion = DateTime.UtcNow;

            await UpdateAsync(espacioId, estadisticas.Id, estadisticas, ct);

            _logger.LogDebug("Karma -{Amount} ? Usuario {Id}: Total={Total}, Semanal={Semanal}, Mensual={Mensual}", 
                karmaAmount, usuarioEspacioId, estadisticas.KarmaTotal, estadisticas.KarmaSemanal, estadisticas.KarmaMensual);

            return estadisticas;
        }

        public async Task UpdateAsync(string espacioId, string id, KarmaEstadisticas entity, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(espacioId)) throw new ArgumentNullException(nameof(espacioId));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var collectionPath = GetKarmaCollectionPath(espacioId);
            var persist = entity.Adapt<FirestoreKarmaEstadisticas>();
            await _firebase.UpdateAsync(collectionPath, id, persist, ct);
        }

        private void ResetIfNewPeriod(KarmaEstadisticas estadisticas)
        {
            int currentWeek = GetCurrentWeek();
            int currentMonth = GetCurrentMonth();

            // Reset semanal
            if (!estadisticas.UltimaSemana.HasValue || estadisticas.UltimaSemana.Value < currentWeek)
            {
                estadisticas.KarmaSemanal = 0;
                estadisticas.UltimaSemana = currentWeek;
                _logger.LogDebug("Karma semanal reseteado para {Id} (semana {Week})", estadisticas.UsuarioEspacioId, currentWeek);
            }

            // Reset mensual
            if (!estadisticas.UltimoMes.HasValue || estadisticas.UltimoMes.Value < currentMonth)
            {
                estadisticas.KarmaMensual = 0;
                estadisticas.UltimoMes = currentMonth;
                _logger.LogDebug("Karma mensual reseteado para {Id} (mes {Month})", estadisticas.UsuarioEspacioId, currentMonth);
            }
        }

        private static int GetCurrentWeek()
        {
            var date = DateTime.UtcNow;
            return System.Globalization.ISOWeek.GetYear(date) * 100 + System.Globalization.ISOWeek.GetWeekOfYear(date);
        }

        private static int GetCurrentMonth() => DateTime.UtcNow.Year * 100 + DateTime.UtcNow.Month;

        // Métodos no soportados de IRepository
        public Task<KarmaEstadisticas?> GetByIdAsync(string id, CancellationToken ct = default) => Task.FromResult<KarmaEstadisticas?>(null);
        public Task<IEnumerable<KarmaEstadisticas>> GetAllAsync(CancellationToken ct = default) => Task.FromResult(Enumerable.Empty<KarmaEstadisticas>());
        public Task UpdateAsync(string id, KarmaEstadisticas entity, CancellationToken ct = default) => throw new NotSupportedException();
        public Task UpdateAsync(string id, KarmaEstadisticas entity, bool merge, CancellationToken ct = default) => throw new NotSupportedException();
        public Task UpdateAsync(string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default) => throw new NotSupportedException();
        public new Task DeleteAsync(string id, CancellationToken ct = default) => throw new NotSupportedException();

        public Task<string> AddAsync(KarmaEstadisticas entitie, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
