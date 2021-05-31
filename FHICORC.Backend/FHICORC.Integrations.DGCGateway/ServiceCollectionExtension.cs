using Microsoft.Extensions.DependencyInjection;
using FHICORC.Integrations.DGCGateway.Services;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using FHICORC.Integrations.DGCGateway.Util;
using FHICORC.Integrations.DGCGateway.Util.Interfaces;

namespace FHICORC.Integrations.DGCGateway
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDgcgGatewayIntegration(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddScoped<IDgcgService, DgcgService>()
                .AddScoped<IDgcgClient, DgcgClient>()
                .AddScoped<IDgcgResponseParser, DgcgResponseParser>()
                .AddScoped<IDgcgResponseVerification, DgcgResponseVerification>()
                .AddScoped<ICertificateVerification, CertificateVerification>();
        }
    }
}
