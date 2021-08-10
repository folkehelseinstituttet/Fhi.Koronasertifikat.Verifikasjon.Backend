using FHICORC.Integrations.UkGateway.Services;
using FHICORC.Integrations.UkGateway.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FHICORC.Integrations.UkGateway
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddUkGatewayIntegration(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddHttpClient()
                .AddScoped<IUkGatewayService, UkGatewayService>();

            return serviceCollection;
        }
    }
}
