using Domain;

namespace Infrastructure.Interfaces
{
    public interface IProductRepository
    {
        public Task<Product> GetByIdAsync(long productId);
    }
}
