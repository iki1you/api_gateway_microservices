namespace Application.DTO.Requests
{
    public sealed record UpdateProductRequest(
        string Name,
        string? Description,
        decimal Price,
        int Count
    );
}
