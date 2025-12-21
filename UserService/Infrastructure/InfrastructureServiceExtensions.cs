using Infrastructure.EF;
using Infrastructure.EF.Repositories;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public class InfrastructureServiceExtensions
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddDbContext<UserDbContext>(options =>
            {
                var connectionString = Environment.GetEnvironmentVariable("DbConnection");
                options.UseNpgsql(connectionString);
            });
            services.AddScoped<IUserRepository, UserRepository>();
        }
    }
}
