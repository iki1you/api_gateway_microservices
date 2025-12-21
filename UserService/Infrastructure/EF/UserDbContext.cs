using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EF
{
    public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
    }
}
