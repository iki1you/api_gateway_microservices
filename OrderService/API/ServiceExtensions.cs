using Application;
using Infrastructure;

namespace API
{
    public class ServiceExtensions
    {
        public static void Configure(IServiceCollection services)
        {
            ApplicationServiceExtensions.Configure(services);
            InfrastructureServiceExtensions.Configure(services);
        }
    }
}
