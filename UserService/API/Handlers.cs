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

        public static void Map(WebApplication app)
        {
            app.MapPost("/api/auth/login", async (LoginRequest request, IHttpClientFactory httpClientFactory) =>
            {
                var client = httpClientFactory.CreateClient("UserService");
                var response = await client.PostAsJsonAsync("/api/users/login", request);

                if (!response.IsSuccessStatusCode)
                    return Results.Unauthorized();

                var user = await response.Content.ReadFromJsonAsync<UserDTO>();

                var token = GenerateJwtToken(app, user!.Id, user.Login);

                return Results.Ok(new LoginResponse(token, user));
            })
            .AllowAnonymous();

            app.MapGet("/api/profile/{userId:long}", async (
                long userId,
                IHttpClientFactory httpClientFactory,
                ILogger<Program> logger) =>
            {
                try
                {
                    var userClient = httpClientFactory.CreateClient("UserService");
                    var userResponse = await userClient.GetAsync($"/api/users/{userId}");

                    if (!userResponse.IsSuccessStatusCode)
                        return Results.NotFound($"User with ID {userId} not found");

                    var user = await userResponse.Content.ReadFromJsonAsync<UserDTO>();

                    var orderClient = httpClientFactory.CreateClient("OrderService");
                    var ordersResponse = await orderClient.GetAsync($"/api/orders/user/{userId}");

                    List<OrderDTO> orders = new();
                    if (ordersResponse.IsSuccessStatusCode)
                    {
                        orders = await ordersResponse.Content.ReadFromJsonAsync<List<OrderDTO>>() ?? new List<OrderDTO>();
                    }

                    var orderProducts = new List<OrderProductResponse>();
                    decimal totalCost = 0;

                    var productClient = httpClientFactory.CreateClient("ProductService");

                    foreach (var order in orders)
                    {
                        var productResponse = await productClient.GetAsync($"/api/products/{order.ProductId}");

                        if (productResponse.IsSuccessStatusCode)
                        {
                            var product = await productResponse.Content.ReadFromJsonAsync<ProductDTO>();
                            if (product != null)
                            {
                                orderProducts.Add(new OrderProductResponse(order.Id, product));
                                totalCost += order.TotalCost;
                            }
                        }
                    }

                    var result = new OrderTotalResponse(user!, totalCost, orderProducts);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error aggregating profile for user {UserId}", userId);
                    return Results.Problem("Internal server error");
                }
            })
            .RequireAuthorization()
            .RequireRateLimiting("Fixed");

            app.MapGet("/api/orders/{orderId:long}", async (
                long orderId,
                IHttpClientFactory httpClientFactory,
                ILogger<Program> logger) =>
            {
                try
                {
                    var orderClient = httpClientFactory.CreateClient("OrderService");
                    var orderResponse = await orderClient.GetAsync($"/api/orders/{orderId}");

                    if (!orderResponse.IsSuccessStatusCode)
                        return Results.NotFound($"Order with ID {orderId} not found");

                    var order = await orderResponse.Content.ReadFromJsonAsync<OrderDTO>();

                    var productClient = httpClientFactory.CreateClient("ProductService");
                    var productResponse = await productClient.GetAsync($"/api/products/{order!.ProductId}");

                    ProductDTO? product = null;
                    if (productResponse.IsSuccessStatusCode)
                    {
                        product = await productResponse.Content.ReadFromJsonAsync<ProductDTO>();
                    }

                    var userClient = httpClientFactory.CreateClient("UserService");
                    var userResponse = await userClient.GetAsync($"/api/users/{order.UserId}");

                    UserDTO? user = null;
                    if (userResponse.IsSuccessStatusCode)
                    {
                        user = await userResponse.Content.ReadFromJsonAsync<UserDTO>();
                    }

                    return Results.Ok(new
                    {
                        Order = order,
                        Product = product,
                        User = user
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error getting order {OrderId}", orderId);
                    return Results.Problem("Internal server error");
                }
            })
            .RequireAuthorization();

            app.MapGet("/api/users/{userId:long}/orders", async (
                long userId,
                IHttpClientFactory httpClientFactory,
                ILogger<Program> logger) =>
            {
                try
                {
                    var orderClient = httpClientFactory.CreateClient("OrderService");
                    var ordersResponse = await orderClient.GetAsync($"/api/orders/user/{userId}");

                    if (!ordersResponse.IsSuccessStatusCode)
                        return Results.Ok(new List<object>());

                    var orders = await ordersResponse.Content.ReadFromJsonAsync<List<OrderDTO>>() ?? new List<OrderDTO>();

                    var productIds = orders.Select(o => o.ProductId).Distinct();
                    var products = new Dictionary<long, ProductDTO>();

                    var productClient = httpClientFactory.CreateClient("ProductService");

                    foreach (var productId in productIds)
                    {
                        var productResponse = await productClient.GetAsync($"/api/products/{productId}");
                        if (productResponse.IsSuccessStatusCode)
                        {
                            var product = await productResponse.Content.ReadFromJsonAsync<ProductDTO>();
                            if (product != null)
                            {
                                products[productId] = product;
                            }
                        }
                    }

                    var result = orders.Select(order =>
                    {
                        products.TryGetValue(order.ProductId, out var product);

                        return new
                        {
                            Order = order,
                            Product = product
                        };
                    }).ToList();

                    return Results.Ok(new
                    {
                        UserId = userId,
                        Orders = result,
                        TotalOrders = orders.Count,
                        TotalCost = orders.Sum(o => o.TotalCost)
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error getting orders for user {UserId}", userId);
                    return Results.Problem("Internal server error");
                }
            })
            .RequireAuthorization();

            app.MapPost("/api/orders", async (
                OrderDTO order,
                IHttpClientFactory httpClientFactory,
                ILogger<Program> logger) =>
            {
                try
                {
                    var userClient = httpClientFactory.CreateClient("UserService");
                    var userResponse = await userClient.GetAsync($"/api/users/{order.UserId}");

                    if (!userResponse.IsSuccessStatusCode)
                        return Results.BadRequest($"User with ID {order.UserId} not found");

                    var productClient = httpClientFactory.CreateClient("ProductService");
                    var productResponse = await productClient.GetAsync($"/api/products/{order.ProductId}");

                    if (!productResponse.IsSuccessStatusCode)
                        return Results.BadRequest($"Product with ID {order.ProductId} not found");

                    var orderClient = httpClientFactory.CreateClient("OrderService");
                    var createResponse = await orderClient.PostAsJsonAsync("/api/orders", order);

                    if (!createResponse.IsSuccessStatusCode)
                        return Results.Problem("Failed to create order");

                    var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDTO>();

                    return Results.Created($"/api/orders/{createdOrder!.Id}", createdOrder);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error creating order");
                    return Results.Problem("Internal server error");
                }
            })
            .RequireAuthorization();
        }

        private static string GenerateJwtToken(WebApplication app, long userId, string username)
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
