using Microsoft.Extensions.DependencyInjection;
using FHICORC.Application.Services.Interfaces;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using FHICORC.Integrations.DGCGateway.Services;

namespace FHICORC.Application.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection, bool useEuDgcg)
        {
            serviceCollection
                .AddScoped<ITextService, TextService>()
                .AddScoped<IRuleService, RuleService>()
                .AddScoped<IRevocationFetchService, RevocationFetchService>()
                .AddScoped<IRevocationUploadService, RevocationUploadService>()
                .AddScoped<IValueSetService, ValueSetService>()
                .AddSingleton<IBloomBucketService, BloomBucketService>();
                

            if (useEuDgcg)
            {
                return serviceCollection
                    .AddScoped<IPublicKeyService, PublicKeyService>()
                    .AddScoped<IJsonPublicKeyService, JsonPublicKeyService>();
            }
            else
            {
                return serviceCollection
                    .AddScoped<IPublicKeyService, JsonPublicKeyService>();
            }
        }
    }
}