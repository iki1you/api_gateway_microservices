using Application.DTO;

namespace Application.Interfaces
{
    public interface IOrderInfoService
    {
        Task<OrderDTO?> GetOrderById(long orderId);
        Task<IReadOnlyCollection<OrderDTO>> GetOrdersByUser(long userId);
        Task<IReadOnlyCollection<OrderDTO>> GetAllOrders();
        Task<OrderDTO> CreateOrder(CreateOrderRequest req);
        Task<OrderDTO?> UpdateOrder(long orderId, UpdateOrderRequest req);
        Task<bool> DeleteOrder(long orderId);
    }
}
