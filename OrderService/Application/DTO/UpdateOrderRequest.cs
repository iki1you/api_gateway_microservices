namespace Application.DTO
{
    public sealed record UpdateOrderRequest(
        long UserId,
        long ProductId,
        int Quantity,
        decimal Price
    );
}

