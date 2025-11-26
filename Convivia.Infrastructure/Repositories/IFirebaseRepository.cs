using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Infrastructure.Repositories
{
    /// <summary>
    /// Contrato para repositorios basados en Firebase / Firestore.
    /// Contiene las operaciones que la capa Infrastructure puede ofrecer
    /// sin exponer detalles de Firestore al resto de la app.
    /// </summary>
    public interface IFirebaseRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserta la entidad y devuelve el id generado por Firestore.
        /// </summary>
        Task<string> AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserta la entidad usando el id que se pasa (útil si queréis ids deterministas).
        /// </summary>
        Task AddWithIdAsync(string id, T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza la entidad con el id indicado.
        /// Por defecto realiza un "replace"; la implementación puede exponer merge si lo soporta.
        /// </summary>
        Task UpdateAsync(string id, T entity, CancellationToken cancellationToken = default);

        Task DeleteAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Consulta por un campo igual a un valor (WHERE field == value).
        /// </summary>
        Task<IEnumerable<T>> QueryByFieldAsync(string field, object value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Consulta con múltiples condiciones AND (cada tupla es field==val).
        /// </summary>
        Task<IEnumerable<T>> QueryMultipleConditionsAsync((string field, object val)[] conditions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cuenta documentos; la implementación puede ser ineficiente para colecciones grandes.
        /// </summary>
        Task<int> CountAsync(CancellationToken cancellationToken = default);
    }
}