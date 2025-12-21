using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EF
{
    public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products => Set<Product>();
    }
}
