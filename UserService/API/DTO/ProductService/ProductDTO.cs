namespace API.DTO.ProductService
{
    public record ProductDTO(long Id, string Name, string? Description, decimal Price);
}
