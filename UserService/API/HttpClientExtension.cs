namespace API
{
    public static class HttpClientExtension
    {
        public static void Configure(WebApplicationBuilder builder)
        {
            builder.Services.AddHttpClient("UserService", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Services:UserService"] ?? "http://localhost:5001");
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            builder.Services.AddHttpClient("OrderService", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Services:OrderService"] ?? "http://localhost:5002");
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            builder.Services.AddHttpClient("ProductService", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Services:ProductService"] ?? "http://localhost:5003");
                client.Timeout = TimeSpan.FromSeconds(10);
            });
        }
    }
}
