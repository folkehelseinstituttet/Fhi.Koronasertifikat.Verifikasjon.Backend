using Microsoft.Extensions.DependencyInjection;
using FHICORC.Application.Services.Interfaces;

namespace FHICORC.Application.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection, bool useEuDgcg)
        {
            serviceCollection
                .AddScoped<ITextService, TextService>()
                .AddScoped<IRuleService, RuleService>()
                .AddScoped<IBloomFilterService, BloomFilterService>()
                .AddScoped<IValueSetService, ValueSetService>();

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