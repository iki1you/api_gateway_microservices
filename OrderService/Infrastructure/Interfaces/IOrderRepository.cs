using Domain;

namespace Infrastructure.Interfaces
{
    public interface IOrderRepository
    {
        public Task<IReadOnlyCollection<Order>> GetListByUserIdAsync(long userId);
    }
}
