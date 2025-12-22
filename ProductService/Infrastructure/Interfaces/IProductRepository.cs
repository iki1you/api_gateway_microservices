using Domain;

namespace Infrastructure.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(long productId, bool includeInactive = false);
        Task<List<Product>> GetAllAsync(bool includeInactive = false);

        Task AddAsync(Product product);
        Task SaveChangesAsync();

        Task<bool> ExistsAsync(long productId, bool includeInactive = false);
    }
}
