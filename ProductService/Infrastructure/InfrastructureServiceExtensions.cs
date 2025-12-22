using Infrastructure.EF;
using Infrastructure.EF.Repositories;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class InfrastructureServiceExtensions
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddDbContext<ProductDbContext>(options =>
            {
                var connectionString = Environment.GetEnvironmentVariable("DbConnection");
                options.UseNpgsql(connectionString);
            });
            services.AddScoped<IProductRepository, ProductRepository>();
        }
    }
}
