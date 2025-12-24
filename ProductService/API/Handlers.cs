using Application.DTO;
using Application.DTO.Requests;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace API;

public static class ProductHandlers
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/products");

        group.MapGet("/", async (IProductInfoService service, IDistributedCache cache) =>
        {
            var cacheKey = "products:all";
            var cached = await cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return Results.Content(cached, "application/json");
            }

            var products = await service.GetProducts();
            var json = JsonSerializer.Serialize(products, JsonOptions);

            await cache.SetStringAsync(
                cacheKey,
                json,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                });

            return Results.Ok(products);
        });

        group.MapGet("/{productId:long}", async (long productId, IProductInfoService service, IDistributedCache cache) =>
        {
            var cacheKey = $"products:id:{productId}";
            var cached = await cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return Results.Content(cached, "application/json");
            }

            var productDto = await service.GetProductInfo(productId);
            if (productDto is null)
            {
                return Results.NotFound();
            }

            var json = JsonSerializer.Serialize(productDto, JsonOptions);

            await cache.SetStringAsync(
                cacheKey,
                json,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

            return Results.Ok(productDto);
        });

        group.MapPost("/", async (CreateProductRequest req, IProductInfoService service, IDistributedCache cache) =>
        {
            var created = await service.CreateProduct(req);

            await cache.RemoveAsync("products:all");

            return Results.Created($"/products/{created.Id}", created);
        });

        group.MapPut("/{productId:long}", async (long productId, UpdateProductRequest req, IProductInfoService service, IDistributedCache cache) =>
        {
            var updated = await service.UpdateProduct(productId, req);
            if (updated is null)
            {
                return Results.NotFound();
            }

            await cache.RemoveAsync("products:all");
            await cache.RemoveAsync($"products:id:{productId}");

            return Results.Ok(updated);
        });

        group.MapDelete("/{productId:long}", async (long productId, IProductInfoService service, IDistributedCache cache) =>
        {
            var deleted = await service.DeleteProduct(productId);
            if (!deleted)
            {
                return Results.NotFound();
            }

            await cache.RemoveAsync("products:all");
            await cache.RemoveAsync($"products:id:{productId}");

            return Results.NoContent();
        });
    }
}
