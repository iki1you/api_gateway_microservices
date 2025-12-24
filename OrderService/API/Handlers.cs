using Application.DTO;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace API
{
    public static class OrderHandlers
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public static void Map(WebApplication app)
        {
            var group = app.MapGroup("/orders");

            group.MapGet("/", async (IOrderInfoService service, IDistributedCache cache) =>
            {
                var cacheKey = "orders:all";
                var cached = await cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    return Results.Content(cached, "application/json");
                }

                var orders = await service.GetAllOrders();
                var json = JsonSerializer.Serialize(orders, JsonOptions);

                await cache.SetStringAsync(
                    cacheKey,
                    json,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
                    });

                return Results.Ok(orders);
            });

            group.MapGet("/{orderId:long}", async (long orderId, IOrderInfoService service, IDistributedCache cache) =>
            {
                var cacheKey = $"orders:id:{orderId}";
                var cached = await cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    return Results.Content(cached, "application/json");
                }

                var orderDto = await service.GetOrderById(orderId);
                if (orderDto is null)
                {
                    return Results.NotFound();
                }

                var json = JsonSerializer.Serialize(orderDto, JsonOptions);

                await cache.SetStringAsync(
                    cacheKey,
                    json,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                    });

                return Results.Ok(orderDto);
            });

            group.MapGet("/user/{userId:long}", async (long userId, IOrderInfoService service, IDistributedCache cache) =>
            {
                var cacheKey = $"orders:user:{userId}";
                var cached = await cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    return Results.Content(cached, "application/json");
                }

                var orders = await service.GetOrdersByUser(userId);
                var json = JsonSerializer.Serialize(orders, JsonOptions);

                await cache.SetStringAsync(
                    cacheKey,
                    json,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
                    });

                return Results.Ok(orders);
            });

            group.MapPost("/", async (CreateOrderRequest req, IOrderInfoService service, IDistributedCache cache) =>
            {
                var created = await service.CreateOrder(req);

                await cache.RemoveAsync("orders:all");

                return Results.Created($"/orders/{created.Id}", created);
            });

            group.MapPatch("/{orderId:long}", async (long orderId, UpdateOrderRequest req, IOrderInfoService service, IDistributedCache cache) =>
            {
                var updated = await service.UpdateOrder(orderId, req);
                if (updated is null)
                {
                    return Results.NotFound();
                }

                await cache.RemoveAsync("orders:all");
                await cache.RemoveAsync($"orders:id:{orderId}");

                return Results.Ok(updated);
            });

            group.MapDelete("/{orderId:long}", async (long orderId, IOrderInfoService service, IDistributedCache cache) =>
            {
                var deleted = await service.DeleteOrder(orderId);
                if (!deleted)
                {
                    return Results.NotFound();
                }

                await cache.RemoveAsync("orders:all");
                await cache.RemoveAsync($"orders:id:{orderId}");

                return Results.NoContent();
            });
        }
    }
}
