using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class ApplicationServiceExtensions
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddScoped<IProductInfoService, ProductInfoService>();
        }
    }
}
