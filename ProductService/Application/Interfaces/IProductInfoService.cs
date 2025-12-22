using Application.DTO;
using Application.DTO.Requests;

namespace Application.Interfaces
{
    public interface IProductInfoService
    {
        Task<ProductDTO?> GetProductInfo(long productId);          
        Task<IReadOnlyList<ProductDTO>> GetProducts();             
        Task<ProductDTO> CreateProduct(CreateProductRequest req);  
        Task<ProductDTO?> UpdateProduct(long productId, UpdateProductRequest req); 
        Task<bool> DeleteProduct(long productId);                  
    }
}
