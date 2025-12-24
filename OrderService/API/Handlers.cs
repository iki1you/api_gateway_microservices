using Application.DTO;
using Application.Interfaces;

namespace API
{
    public static class OrderHandlers
    {
        public static void Map(WebApplication app)
        {
            var group = app.MapGroup("/orders");

            group.MapGet("/", async (IOrderInfoService service) =>
            {
                var orders = await service.GetAllOrders();
                return Results.Ok(orders);
            });

            group.MapGet("/{orderId:long}", async (long orderId, IOrderInfoService service) =>
            {
                var orderDto = await service.GetOrderById(orderId);
                return orderDto is null ? Results.NotFound() : Results.Ok(orderDto);
            });

            group.MapGet("/user/{userId:long}", async (long userId, IOrderInfoService service) =>
            {
                var orders = await service.GetOrdersByUser(userId);
                return Results.Ok(orders);
            });

            group.MapPost("/", async (CreateOrderRequest req, IOrderInfoService service) =>
            {
                var created = await service.CreateOrder(req);
                return Results.Created($"/orders/{created.Id}", created);
            });

            group.MapPatch("/{orderId:long}", async (long orderId, UpdateOrderRequest req, IOrderInfoService service) =>
            {
                var updated = await service.UpdateOrder(orderId, req);
                return updated is null ? Results.NotFound() : Results.Ok(updated);
            });

            group.MapDelete("/{orderId:long}", async (long orderId, IOrderInfoService service) =>
            {
                var deleted = await service.DeleteOrder(orderId);
                return deleted ? Results.NoContent() : Results.NotFound();
            });
        }
    }
}
