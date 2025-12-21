using Application.Interfaces;

namespace API
{
    public static class ProductHandlers
    {
        public static void Map(WebApplication app)
        {
            app.MapGet("/{productId:long}", async (long productId) =>
            {
                var service = app.Services.GetRequiredService<IProductInfoService>();
                var productDto = await service.GetProductInfo(productId);

                return Results.Ok(productDto);
            });
        }
    }
}
