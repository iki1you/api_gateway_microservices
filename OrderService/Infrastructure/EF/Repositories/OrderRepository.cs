using Domain;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EF.Repositories
{
    public class OrderRepository(OrderDbContext dbContext) : IOrderRepository
    {
        private OrderDbContext _dbContext = dbContext;

        public async Task<IReadOnlyCollection<Order>> GetListByUserIdAsync(long userId) =>
            await _dbContext.Orders.Where(x => x.UserId == userId).ToListAsync();
    }
}
