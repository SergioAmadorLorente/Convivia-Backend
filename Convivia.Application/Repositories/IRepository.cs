namespace Convivia.Application.Repositories
{
    public interface IRepository<T>
    {
        Task<T?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<string> AddAsync(T entitie, CancellationToken ct = default);

        // Aqui tenim diferents update:

        // Update completo (overwrite por defecto)
        Task UpdateAsync(string id, T entitie, CancellationToken ct = default);

        // Update con opción merge (SetOptions.MergeAll en Firestore)
        Task UpdateAsync(string id, T entitie, bool merge, CancellationToken ct = default);

        // Update parcial mediante diccionario (útil para PATCH)
        Task UpdateAsync(string id, IDictionary<string, object> updates, bool useSetMerge = true, CancellationToken ct = default);

        Task DeleteAsync(string id, CancellationToken ct = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    }
}
