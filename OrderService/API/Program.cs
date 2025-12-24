using API;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ServiceExtensions.Configure(builder.Services);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().ForwardToPrometheus();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseHttpMetrics();

app.UseHttpsRedirection();

app.UseAuthorization();

OrderHandlers.Map(app);
app.MapControllers();
app.MapMetrics();

app.Run();
