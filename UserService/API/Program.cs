using API;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

ServiceExtentions.Configure(builder.Services);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "ApiGateway:";
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().ForwardToPrometheus();

var app = builder.Build();

UserHandlers.Map(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseHttpMetrics();
app.UseHttpsRedirection();
app.MapMetrics();

app.Run();
