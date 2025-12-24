using Domain;

namespace Infrastructure.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(long orderId, bool includeInactive = false);
        Task<IReadOnlyCollection<Order>> GetListByUserIdAsync(long userId);
        Task<List<Order>> GetAllAsync(bool includeInactive = false);
        Task AddAsync(Order order);
        Task SaveChangesAsync();
        Task<bool> ExistsAsync(long orderId, bool includeInactive = false);
    }
}
