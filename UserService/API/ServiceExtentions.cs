using Infrastructure;
using Application;

namespace API
{
    public static class ServiceExtentions
    {
        public static void Configure(IServiceCollection services)
        {
            InfrastructureServiceExtensions.Configure(services);
            ApplicationServiceExtensions.Configure(services);
        }
    }
}
