namespace API.DTO.OrderService
{
    public record OrderDTO(long Id, long UserId, long ProductId, int Quantity, decimal TotalCost);
}
