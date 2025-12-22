using Application.DTO;
using Application.DTO.Requests;
using Application.Interfaces;

namespace API;

public static class ProductHandlers
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/products");

        group.MapGet("/", async (IProductInfoService service) =>
        {
            var products = await service.GetProducts();
            return Results.Ok(products);
        });

        group.MapGet("/{productId:long}", async (long productId, IProductInfoService service) =>
        {
            var productDto = await service.GetProductInfo(productId);
            return productDto is null ? Results.NotFound() : Results.Ok(productDto);
        });

        group.MapPost("/", async (CreateProductRequest req, IProductInfoService service) =>
        {
            var created = await service.CreateProduct(req);
            return Results.Created($"/products/{created.Id}", created);
        });

        group.MapPut("/{productId:long}", async (long productId, UpdateProductRequest req, IProductInfoService service) =>
        {
            var updated = await service.UpdateProduct(productId, req);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        group.MapDelete("/{productId:long}", async (long productId, IProductInfoService service) =>
        {
            var deleted = await service.DeleteProduct(productId);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}
