using Application.DTO;

namespace Application.Interfaces
{
    public interface IOrderInfoService
    {
        public Task<IReadOnlyCollection<OrderDTO>> GetOrdersByUser(long UserId);
    }
}
