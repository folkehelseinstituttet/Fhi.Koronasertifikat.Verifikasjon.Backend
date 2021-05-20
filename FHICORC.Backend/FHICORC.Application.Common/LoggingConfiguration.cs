using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System;
using System.IO;

namespace FHICORC.Application.Common
{
    public static class LoggingConfiguration
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        public static IHostBuilder UseSerilogConfiguration(this IHostBuilder hostBuilder, string serviceName)
        {
            return hostBuilder
                .ConfigureServices((context, collection) => collection.AddLogging(builder => builder.ClearProviders()))
                .UseSerilog(ConfigureSerilog(serviceName));
        }

        private static Action<HostBuilderContext, LoggerConfiguration> ConfigureSerilog(string serviceName)
        {
            return (hostingContext, loggerConfiguration) =>
            {

                var configuration = loggerConfiguration
                    .ReadFrom.Configuration(Configuration)
                    .Enrich.FromLogContext().Destructure.ToMaximumDepth(5)
                    .Enrich.WithProperty(nameof(Environment.MachineName), Environment.MachineName)
                    .Enrich.WithProperty("ServiceName", serviceName)
                    .Destructure.ToMaximumDepth(10);

                configuration.WriteTo.Async(x => x.Console(
                    formatter: new JsonFormatter(),
                    standardErrorFromLevel: LogEventLevel.Error));


#if DEBUG
                configuration.WriteTo.Async(x => x.Debug());
#endif
            };
        }
    }
}