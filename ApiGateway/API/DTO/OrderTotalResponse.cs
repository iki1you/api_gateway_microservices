using API.DTO.UserService;

namespace API.DTO
{
    public record OrderTotalResponse(UserDTO User, decimal TotalCost, List<OrderProductResponse> orders);
}
