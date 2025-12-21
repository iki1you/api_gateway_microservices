using API;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

ServiceExtentions.Configure(builder.Services);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().ForwardToPrometheus();

var app = builder.Build();

UserHandlers.Map(app);

// Configure the HTTP request pipeline.
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
