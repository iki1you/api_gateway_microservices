using Application.DTO;

namespace Application.Interfaces
{
    public interface IProductInfoService
    {
        public Task<ProductDTO> GetProductInfo(long productId);
    }
}
