using Application.DTO;
using Application.Interfaces;
using Infrastructure.Interfaces;

namespace Application.Services
{
    public class OrderInfoService(IOrderRepository orderRepository) : IOrderInfoService
    {
        private IOrderRepository _orderRepository = orderRepository;

        public async Task<IReadOnlyCollection<OrderDTO>> GetOrdersByUser(long UserId)
        {
            var orders = await _orderRepository.GetListByUserIdAsync(UserId);

            return orders.Select(x => 
                        new OrderDTO(x.Id, x.UserId, x.ProductId, x.Quantity, x.TotalCost)
                   ).ToList();
        }
    }
}
