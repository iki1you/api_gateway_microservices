namespace Application.DTO
{
    public record OrderDTO(long Id, long UserId, long ProductId, int Quantity, decimal TotalCost);
}
