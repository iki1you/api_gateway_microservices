using Domain;

namespace Infrastructure.Interfaces
{
    public interface IUserRepository
    {
        public Task<User> GetByIdAsync(long userId);
    }
}
