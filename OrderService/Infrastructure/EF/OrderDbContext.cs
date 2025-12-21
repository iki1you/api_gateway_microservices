using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EF
{
    public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();
    }
}
