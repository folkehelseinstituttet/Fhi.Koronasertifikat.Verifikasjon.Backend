using Microsoft.Extensions.DependencyInjection;
using FHICORC.Application.Services.Interfaces;

namespace FHICORC.Application.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection, bool useEuDgcg)
        {
            if (useEuDgcg)
            {
                return serviceCollection
                    .AddScoped<IPublicKeyService, PublicKeyService>()
                    .AddScoped<IJsonPublicKeyService, JsonPublicKeyService>()
                    .AddScoped<ITextService, TextService>();
            }
            else
            {
                return serviceCollection
                    .AddScoped<IPublicKeyService, JsonPublicKeyService>()
                    .AddScoped<ITextService, TextService>();
            }
        }
    }
}