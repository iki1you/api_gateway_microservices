using Application.DTO;
using Application.Interfaces;
using Infrastructure.Interfaces;

namespace Application.Services
{
    public class ProductInfoService(IProductRepository productRepository) : IProductInfoService
    {
        private IProductRepository _productRepository = productRepository;

        public async Task<ProductDTO> GetProductInfo(long productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            return new ProductDTO(product.Id, product.Name, product.Description, product.Price, product.Count);
        }
    }
}
