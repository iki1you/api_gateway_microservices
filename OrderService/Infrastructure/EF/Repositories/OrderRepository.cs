using Domain;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EF.Repositories
{
    public class OrderRepository(OrderDbContext dbContext) : IOrderRepository
    {
        private readonly OrderDbContext _dbContext = dbContext;

        public Task<Order?> GetByIdAsync(long orderId, bool includeInactive = false)
        {
            return _dbContext.Orders.FirstOrDefaultAsync(o =>
                o.Id == orderId && (includeInactive || o.IsActive));
        }

        public async Task<IReadOnlyCollection<Order>> GetListByUserIdAsync(long userId)
        {
            return await _dbContext.Orders
                .Where(x => x.UserId == userId && x.IsActive)
                .ToListAsync();
        }

        public Task<List<Order>> GetAllAsync(bool includeInactive = false)
        {
            return _dbContext.Orders
                .Where(o => includeInactive || o.IsActive)
                .OrderBy(o => o.Id)
                .ToListAsync();
        }

        public async Task AddAsync(Order order)
        {
            await _dbContext.Orders.AddAsync(order);
        }

        public Task SaveChangesAsync()
        {
            return _dbContext.SaveChangesAsync();
        }

        public Task<bool> ExistsAsync(long orderId, bool includeInactive = false)
        {
            return _dbContext.Orders.AnyAsync(o =>
                o.Id == orderId && (includeInactive || o.IsActive));
        }
    }
}
