using Application.DTO;
using Application.DTO.Requests;
using Application.Interfaces;
using Domain;
using Infrastructure.Interfaces;

namespace Application.Services;

public sealed class ProductInfoService(IProductRepository productRepository) : IProductInfoService
{
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<ProductDTO?> GetProductInfo(long productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        return product is null ? null : ToDto(product);
    }

    public async Task<IReadOnlyList<ProductDTO>> GetProducts()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(ToDto).ToList();
    }

    public async Task<ProductDTO> CreateProduct(CreateProductRequest req)
    {
        var product = new Product
        {
            Name = req.Name,
            Description = req.Description,
            Price = req.Price,
            Count = req.Count,
            IsActive = true
        };

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        return ToDto(product);
    }

    public async Task<ProductDTO?> UpdateProduct(long productId, UpdateProductRequest req)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null) return null;

        product.Name = req.Name;
        product.Description = req.Description;
        product.Price = req.Price;
        product.Count = req.Count;

        await _productRepository.SaveChangesAsync();
        return ToDto(product);
    }

    public async Task<bool> DeleteProduct(long productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null) return false;

        product.IsActive = false;

        await _productRepository.SaveChangesAsync();
        return true;
    }

    private static ProductDTO ToDto(Product p)
        => new ProductDTO(p.Id, p.Name, p.Description, p.Price, p.Count);
}
