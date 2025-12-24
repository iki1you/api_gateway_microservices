namespace API.DTO.OrderService
{
    public record OrderDTO(long ProductId, int Quantity, decimal Price, decimal TotalCost);
}
