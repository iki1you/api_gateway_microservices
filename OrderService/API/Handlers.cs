using Application.Interfaces;

namespace API
{
    public static class OrderHandlers
    {
        public static void Map(WebApplication app)
        {
            app.MapGet("/{userId:long}", async (long userId) =>
            {
                var service = app.Services.GetRequiredService<IOrderInfoService>();
                var orders = await service.GetOrdersByUser(userId);

                return Results.Ok(orders);
            });
        }
    }
}
