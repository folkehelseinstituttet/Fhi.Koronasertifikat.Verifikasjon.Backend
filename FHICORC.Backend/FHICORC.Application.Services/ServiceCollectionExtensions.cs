using Microsoft.Extensions.DependencyInjection;
using FHICORC.Application.Services.Interfaces;

namespace FHICORC.Application.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddScoped<IPublicKeyService, PublicKeyService>()
                .AddScoped<ITextService, TextService>();
        }
    }
}