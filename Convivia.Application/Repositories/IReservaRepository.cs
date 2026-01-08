using Convivia.Domain.Entities;

namespace Convivia.Application.Repositories
{
    public interface IReservaRepository : IRepository<Reserva>
    {
        public Task<IEnumerable<Reserva>> GetByUsuarioIdAsync(string usuarioId, CancellationToken ct = default);
        public Task<IEnumerable<Reserva>> GetBySalaIdAsync(string salaId, CancellationToken ct = default);
    }
}
