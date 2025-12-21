using Application.Interfaces;

namespace API
{
    public static class UserHandlers
    {
        public static void Map(WebApplication app)
        {
            app.MapGet("/{userId:long}", async (long userId) =>
            {
                var service = app.Services.GetRequiredService<IUserInfoService>();
                var userDto = await service.GetUserInfo(userId);

                return Results.Ok(userDto);
            });
        }
    }
}
