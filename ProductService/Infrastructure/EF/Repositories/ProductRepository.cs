using Domain;
using Infrastructure.Interfaces;

namespace Infrastructure.EF.Repositories
{
    public class ProductRepository : IProductRepository
    {
        public Task<Product> GetByIdAsync(long productId)
        {
            throw new NotImplementedException();
        }
    }
}
