namespace Application.DTO
{
    public sealed record CreateOrderRequest(
        long UserId,
        long ProductId,
        int Quantity,
        decimal Price
    );
}

