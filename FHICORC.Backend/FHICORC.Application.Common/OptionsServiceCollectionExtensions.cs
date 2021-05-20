using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FHICORC.Application.Common
{
    public static class OptionsServiceCollectionExtensions
    {
        public static IServiceCollection AddValidatedOptions<TOptions>(this IServiceCollection serviceCollection, IConfiguration configuration, string rootSectionPrefix = null)
            where TOptions : class, new()
        {
            var sectionKey = rootSectionPrefix != null ? $"{rootSectionPrefix}:{typeof(TOptions).Name}" : typeof(TOptions).Name;

            serviceCollection.AddOptions<TOptions>()
                .Bind(configuration.GetSection(sectionKey))
                .ValidateDataAnnotations();
            serviceCollection.AddSingleton(provider => provider.GetRequiredService<IOptionsMonitor<TOptions>>().CurrentValue);
            return serviceCollection;
        }
        public static IServiceCollection AddValidatedOptions<TOptions>(this IServiceCollection serviceCollection, IConfigurationSection configurationSection)
            where TOptions : class, new()
        {
            serviceCollection.AddOptions<TOptions>()
                .Bind(configurationSection)
                .ValidateDataAnnotations();
            serviceCollection.AddSingleton(provider => provider.GetRequiredService<IOptionsMonitor<TOptions>>().CurrentValue);
            return serviceCollection;
        }
        public static TOptions RetrieveOptions<TOptions>(this IConfiguration configuration, string rootSectionPrefix = null)
            where TOptions : class, new()
        {
            var sectionKey = rootSectionPrefix != null ? $"{rootSectionPrefix}:{typeof(TOptions).Name}" : typeof(TOptions).Name;
            return configuration.GetSection(sectionKey).Get<TOptions>();
        }
    }
}