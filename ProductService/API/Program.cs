using API;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks().ForwardToPrometheus();

ServiceExtensions.Configure(builder.Services);

var app = builder.Build();

app.UseRouting();

app.UseHttpMetrics();

app.UseHttpsRedirection();

app.MapMetrics();

app.Run();
