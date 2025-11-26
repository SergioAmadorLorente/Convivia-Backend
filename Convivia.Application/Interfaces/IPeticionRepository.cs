using Convivia.Domain.Models;

namespace Convivia.Application.Interfaces
{
    /// <summary>
    /// Contrato de repositorio para peticiones
    /// Define operaciones CRUD sin exponer detalles de implementación
    /// </summary>
    public interface IPeticionRepository
    {
        Task<Peticion> AddAsync(Peticion peticion);
        Task<Peticion?> GetByIdAsync(string id);
        Task<List<Peticion>> GetAllAsync();
        Task<List<Peticion>> GetByEstadoAsync(string estado);
        Task<Peticion> UpdateAsync(Peticion peticion);
        Task<bool> DeleteAsync(string id);
    }
}
