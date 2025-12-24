using API.DTO.OrderService;
using API.DTO.ProductService;

namespace API.DTO
{
    public record OrderProductResponse(OrderDTO OrderItemDTO, ProductDTO ProductDTO);
}
