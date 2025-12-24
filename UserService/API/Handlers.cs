using API.DTO;
using API.DTO.OrderService;
using API.DTO.ProductService;
using API.DTO.UserService;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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
                ILogger<Program> logger,
                IDistributedCache cache) =>
            {
                try
                {
                    logger.LogInformation("Aggregating profile for user {UserId}", userId);

                    var cacheKey = $"profile:{userId}";
                    var cachedJson = await cache.GetStringAsync(cacheKey);
                    if (!string.IsNullOrEmpty(cachedJson))
                    {
                        return Results.Content(cachedJson, "application/json");
                    }

                    var userTask = GetUserAsync(userId, httpClientFactory, cache);
                    var ordersTask = GetUserOrdersAsync(userId, httpClientFactory, cache);

                    await Task.WhenAll(userTask, ordersTask);

                    var user = await userTask;
                    var orders = await ordersTask;

                    if (user == null)
                    {
                        return Results.NotFound(new ErrorResponse($"User with ID {userId} not found"));
                    }

                    var profile = new
                    {
                        User = user,
                        Orders = orders,
                        TotalOrders = orders.Count,
                        TotalAmount = orders.Sum(o => o.TotalCost)
                    };

                    var json = JsonSerializer.Serialize(profile, JsonOptions);
                    await cache.SetStringAsync(
                        cacheKey,
                        json,
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                        });

                    return Results.Ok(profile);
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, "Error calling microservice for user {UserId}", userId);
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
                ILogger<Program> logger,
                IDistributedCache cache) =>
            {
                try
                {
                    var orders = await GetUserOrdersAsync(userId, httpClientFactory, cache);
                    var enrichedOrders = new List<OrderTotalDTO>();

                    foreach (var order in orders)
                    {
                        var productIds = order.OrderItems.Select(i => i.ProductId).Distinct().ToList();
                        _ = await GetProductsAsync(productIds, httpClientFactory, cache);
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
                ILogger<Program> logger,
                IDistributedCache cache) =>
            {
                try
                {
                    var normalized = (query ?? string.Empty).Trim().ToLowerInvariant();
                    var cacheKey = $"users:search:{normalized}";

                    var cached = await GetFromCacheAsync<List<UserDTO>>(cache, cacheKey);
                    if (cached != null)
                    {
                        return Results.Ok(cached);
                    }

                    var client = httpClientFactory.CreateClient("UserService");
                    var response = await client.GetAsync($"/api/users/search?q={Uri.EscapeDataString(query)}");

                    if (response.IsSuccessStatusCode)
                    {
                        var users = await response.Content.ReadFromJsonAsync<List<UserDTO>>() ?? new List<UserDTO>();

                        await SetToCacheAsync(cache, cacheKey, users, TimeSpan.FromSeconds(30));
                        return Results.Ok(users);
                    }

                    await SetToCacheAsync(cache, cacheKey, new List<UserDTO>(), TimeSpan.FromSeconds(15));
                    return Results.Ok(new List<UserDTO>());
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error searching users with query {Query}", query);
                    return Results.Problem(ex.Message);
                }
            })
            .RequireAuthorization();

            async Task<UserDTO?> GetUserAsync(long userId, IHttpClientFactory httpClientFactory, IDistributedCache cache)
            {
                var cacheKey = $"user:{userId}";
                var cached = await GetFromCacheAsync<UserDTO>(cache, cacheKey);
                if (cached != null)
                {
                    return cached;
                }

                var client = httpClientFactory.CreateClient("UserService");
                var response = await client.GetAsync($"/api/users/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<UserDTO>();
                    if (user != null)
                    {
                        await SetToCacheAsync(cache, cacheKey, user, TimeSpan.FromSeconds(60));
                    }
                    return user;
                }

                return null;
            }

            async Task<List<OrderTotalDTO>> GetUserOrdersAsync(long userId, IHttpClientFactory httpClientFactory, IDistributedCache cache)
            {
                var cacheKey = $"orders:user:{userId}";
                var cached = await GetFromCacheAsync<List<OrderTotalDTO>>(cache, cacheKey);
                if (cached != null)
                {
                    return cached;
                }

                try
                {
                    var client = httpClientFactory.CreateClient("OrderService");
                    var response = await client.GetAsync($"/api/orders/user/{userId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var orders = await response.Content.ReadFromJsonAsync<List<OrderTotalDTO>>() ?? new List<OrderTotalDTO>();
                        await SetToCacheAsync(cache, cacheKey, orders, TimeSpan.FromSeconds(30));
                        return orders;
                    }

                    return new List<OrderTotalDTO>();
                }
                catch
                {
                    return new List<OrderTotalDTO>();
                }
            }

            async Task<List<ProductDTO>> GetProductsAsync(List<long> productIds, IHttpClientFactory httpClientFactory, IDistributedCache cache)
            {
                if (!productIds.Any())
                {
                    return new List<ProductDTO>();
                }

                var normalizedIds = productIds.Distinct().OrderBy(x => x).ToArray();
                var cacheKey = $"products:batch:{HashIds(normalizedIds)}";

                var cached = await GetFromCacheAsync<List<ProductDTO>>(cache, cacheKey);
                if (cached != null)
                {
                    return cached;
                }

                try
                {
                    var client = httpClientFactory.CreateClient("ProductService");
                    var request = new { ProductIds = normalizedIds };
                    var response = await client.PostAsJsonAsync("/api/products/batch", request);

                    if (response.IsSuccessStatusCode)
                    {
                        var products = await response.Content.ReadFromJsonAsync<List<ProductDTO>>() ?? new List<ProductDTO>();
                        await SetToCacheAsync(cache, cacheKey, products, TimeSpan.FromMinutes(5));

                        foreach (var p in products)
                        {
                            await SetToCacheAsync(cache, $"product:{p.Id}", p, TimeSpan.FromMinutes(10));
                        }

                        return products;
                    }

                    var productTasks = normalizedIds.Select(id => GetProductAsync(id, httpClientFactory, cache));
                    var productsArray = await Task.WhenAll(productTasks);

                    var list = productsArray.Where(p => p != null).Select(p => p!).ToList();
                    await SetToCacheAsync(cache, cacheKey, list, TimeSpan.FromMinutes(2));
                    return list;
                }
                catch
                {
                    return new List<ProductDTO>();
                }
            }

            async Task<ProductDTO?> GetProductAsync(long productId, IHttpClientFactory httpClientFactory, IDistributedCache cache)
            {
                var cacheKey = $"product:{productId}";
                var cached = await GetFromCacheAsync<ProductDTO>(cache, cacheKey);
                if (cached != null)
                {
                    return cached;
                }

                try
                {
                    var client = httpClientFactory.CreateClient("ProductService");
                    var response = await client.GetAsync($"/api/products/{productId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var product = await response.Content.ReadFromJsonAsync<ProductDTO>();
                        if (product != null)
                        {
                            await SetToCacheAsync(cache, cacheKey, product, TimeSpan.FromMinutes(10));
                        }
                        return product;
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

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private static async Task<T?> GetFromCacheAsync<T>(IDistributedCache cache, string key)
        {
            var json = await cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(json, JsonOptions);
            }
            catch
            {
                return default;
            }
        }

        private static async Task SetToCacheAsync<T>(IDistributedCache cache, string key, T value, TimeSpan ttl)
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            await cache.SetStringAsync(
                key,
                json,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                });
        }

        private static string HashIds(IEnumerable<long> ids)
        {
            var payload = string.Join(",", ids);
            var bytes = Encoding.UTF8.GetBytes(payload);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
