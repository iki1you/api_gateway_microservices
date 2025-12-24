using Application.DTO;
using Application.Interfaces;
using Domain;
using Infrastructure.Interfaces;

namespace Application.Services
{
    public class OrderInfoService(IOrderRepository orderRepository) : IOrderInfoService
    {
        private readonly IOrderRepository _orderRepository = orderRepository;

        public async Task<OrderDTO?> GetOrderById(long orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            return order is null ? null : ToDto(order);
        }

        public async Task<IReadOnlyCollection<OrderDTO>> GetOrdersByUser(long userId)
        {
            var orders = await _orderRepository.GetListByUserIdAsync(userId);
            return orders.Select(ToDto).ToList();
        }

        public async Task<IReadOnlyCollection<OrderDTO>> GetAllOrders()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(ToDto).ToList();
        }

        public async Task<OrderDTO> CreateOrder(CreateOrderRequest req)
        {
            var order = new Order
            {
                UserId = req.UserId,
                ProductId = req.ProductId,
                Quantity = req.Quantity,
                Price = req.Price,
                TotalCost = req.Price * req.Quantity,
                IsActive = true
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            return ToDto(order);
        }

        public async Task<OrderDTO?> UpdateOrder(long orderId, UpdateOrderRequest req)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order is null) return null;

            order.UserId = req.UserId;
            order.ProductId = req.ProductId;
            order.Quantity = req.Quantity;
            order.Price = req.Price;
            order.TotalCost = req.Price * req.Quantity;

            await _orderRepository.SaveChangesAsync();
            return ToDto(order);
        }

        public async Task<bool> DeleteOrder(long orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order is null) return false;

            order.IsActive = false;

            await _orderRepository.SaveChangesAsync();
            return true;
        }

        private static OrderDTO ToDto(Order o)
            => new OrderDTO(o.Id, o.UserId, o.ProductId, o.Quantity, o.TotalCost);
    }
}
