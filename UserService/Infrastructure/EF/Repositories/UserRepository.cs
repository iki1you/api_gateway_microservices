using Domain;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EF.Repositories
{
    public class UserRepository : IUserRepository
    {
        private UserDbContext _dbContext;

        public UserRepository(UserDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task<User> GetByIdAsync(long userId)
        {
            return await _dbContext.Users.FirstAsync(x => x.Id == userId);
        }
    }
}
