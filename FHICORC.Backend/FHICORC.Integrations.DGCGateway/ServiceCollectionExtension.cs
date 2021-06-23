using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using FHICORC.Integrations.DGCGateway.Services;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using FHICORC.Integrations.DGCGateway.Util;
using FHICORC.Integrations.DGCGateway.Util.Interfaces;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace FHICORC.Integrations.DGCGateway
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDgcgGatewayIntegration(this IServiceCollection serviceCollection, bool useBouncyCastle)
        {
            serviceCollection
                .AddScoped<IDgcgService, DgcgService>()
                .AddScoped<IDgcgClient, DgcgClient>()
                .AddScoped<IDgcgResponseParser, DgcgResponseParser>();

            if (useBouncyCastle)
            {
                serviceCollection.AddScoped<IDgcgResponseVerification, DgcgResponseVerification<X509Certificate>>()
                    .AddScoped<ICertificateVerification<X509Certificate>, BcCertificateVerification>();
            }
            else
            {
                serviceCollection.AddScoped<IDgcgResponseVerification, DgcgResponseVerification<X509Certificate2>>()
                    .AddScoped<ICertificateVerification<X509Certificate2>, CertificateVerification>();
            }

            return serviceCollection;
        }
    }
}
