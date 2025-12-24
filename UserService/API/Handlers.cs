using API.DTO;
using API.DTO.OrderService;
using API.DTO.ProductService;
using API.DTO.UserService;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API
{
    public static class UserHandlers
    {
        public record ErrorResponse(string Message);

        public static void Map(WebApplication app)
        {
            app.MapPost("/api/auth/login", async (LoginRequest request, IHttpClientFactory httpClientFactory) =>
            {
                try
                {
                    var client = httpClientFactory.CreateClient("UserService");
                    var response = await client.PostAsJsonAsync("/api/users/authenticate", request);

                    if (!response.IsSuccessStatusCode)
                    {
                        return Results.Unauthorized();
                    }

                    var user = await response.Content.ReadFromJsonAsync<UserDTO>();

                    var token = GenerateJwtToken(user.Id, user.Login);

                    return Results.Ok(new LoginResponse(token, user));
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .AllowAnonymous();

            app.MapGet("/api/profile/{userId:long}", async (
                long userId,
                IHttpClientFactory httpClientFactory,
                ILogger<Program> logger) =>
            {
                try
                {
                    logger.LogInformation("Aggregating profile for user {UserId}", userId);

                    var userTask = GetUserAsync(userId, httpClientFactory);
                    var ordersTask = GetUserOrdersAsync(userId, httpClientFactory);

                    await Task.WhenAll(userTask, ordersTask);

                    var user = await userTask;
                    var orders = await ordersTask;

                    if (user == null)
                    {
                        return Results.NotFound(new ErrorResponse($"User with ID {userId} not found"));
                    }

                    //var orderTotals = orders.Select(x => new OrderProductResponse
                    //{
                    //       
                    //});
                    //
                    //var products = await GetProductsAsync(productIds, httpClientFactory);
                    //
                    //var profile = new OrderTotalResponse(user, 1, orders);

                    return Results.Ok();
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, "Error calling microservice for user {UserId}", userId);

                    // Fallback логика
                    return Results.BadRequest();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error for user {UserId}", userId);
                    return Results.Problem("Internal server error");
                }
            })
            .RequireAuthorization()
            .RequireRateLimiting("Fixed");

            app.MapGet("/api/users/{userId:long}/orders", async (
                long userId,
                IHttpClientFactory httpClientFactory,
                ILogger<Program> logger) =>
            {
                try
                {
                    var orders = await GetUserOrdersAsync(userId, httpClientFactory);

                    var enrichedOrders = new List<OrderTotalDTO>();

                    foreach (var order in orders)
                    {
                        var productIds = order.OrderItems.Select(i => i.ProductId).Distinct().ToList();
                        var products = await GetProductsAsync(productIds, httpClientFactory);

                        enrichedOrders.Add(order);
                    }

                    return Results.Ok(new
                    {
                        UserId = userId,
                        Orders = enrichedOrders,
                        TotalOrders = enrichedOrders.Count,
                        TotalAmount = enrichedOrders.Sum(o => o.TotalCost)
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error getting orders for user {UserId}", userId);
                    return Results.Problem(ex.Message);
                }
            })
            .RequireAuthorization();
            
            app.MapGet("/api/users/search", async (
                string query,
                IHttpClientFactory httpClientFactory,
                ILogger<Program> logger) =>
            {
                try
                {
                    var client = httpClientFactory.CreateClient("UserService");
                    var response = await client.GetAsync($"/api/users/search?q={Uri.EscapeDataString(query)}");

                    if (response.IsSuccessStatusCode)
                    {
                        var users = await response.Content.ReadFromJsonAsync<List<UserDTO>>();
                        return Results.Ok(users);
                    }

                    return Results.Ok(new List<UserDTO>());
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error searching users with query {Query}", query);
                    return Results.Problem(ex.Message);
                }
            })
            .RequireAuthorization();

            async Task<UserDTO?> GetUserAsync(long userId, IHttpClientFactory httpClientFactory)
            {
                var client = httpClientFactory.CreateClient("UserService");
                var response = await client.GetAsync($"/api/users/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserDTO>();
                }

                return null;
            }

            async Task<List<OrderTotalDTO>> GetUserOrdersAsync(long userId, IHttpClientFactory httpClientFactory)
            {
                try
                {
                    var client = httpClientFactory.CreateClient("OrderService");
                    var response = await client.GetAsync($"/api/orders/user/{userId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var orders = await response.Content.ReadFromJsonAsync<List<OrderTotalDTO>>();
                        return orders ?? [];
                    }

                    return [];
                }
                catch
                {
                    return [];
                }
            }

            async Task<List<ProductDTO>> GetProductsAsync(List<long> productIds, IHttpClientFactory httpClientFactory)
            {
                if (!productIds.Any())
                {
                    return [];
                }

                try
                {
                    var client = httpClientFactory.CreateClient("ProductService");

                    var request = new { ProductIds = productIds };
                    var response = await client.PostAsJsonAsync("/api/products/batch", request);

                    if (response.IsSuccessStatusCode)
                    {
                        var products = await response.Content.ReadFromJsonAsync<List<ProductDTO>>();
                        return products ?? [];
                    }

                    var productTasks = productIds.Select(id => GetProductAsync(id, httpClientFactory));
                    var productsArray = await Task.WhenAll(productTasks);

                    return productsArray.Where(p => p != null).Select(p => p!).ToList();
                }
                catch
                {
                    return [];
                }
            }

            async Task<ProductDTO?> GetProductAsync(long productId, IHttpClientFactory httpClientFactory)
            {
                try
                {
                    var client = httpClientFactory.CreateClient("ProductService");
                    var response = await client.GetAsync($"/api/products/{productId}");

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadFromJsonAsync<ProductDTO>();
                    }

                    return null;
                }
                catch
                {
                    return null;
                }
            }

            string GenerateJwtToken(long userId, string username)
            {
                var jwtSettings = app.Configuration.GetSection("Jwt");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"],
                    audience: jwtSettings["Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
        }
    }
}
