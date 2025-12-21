namespace API.DTO.OrderService
{
    public record OrderTotalDTO(long Id, long UserId, DateTime OrderDate, decimal TotalCost, List<OrderDTO> OrderItems);
}
