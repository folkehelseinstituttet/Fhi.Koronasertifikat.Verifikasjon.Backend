using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FHICORC.Application.Common;
using FHICORC.Application.Common.Interfaces;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Repositories;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Application.Services;
using FHICORC.Application.Validation;
using FHICORC.ApplicationHost.Api.Middleware;

namespace FHICORC.ApplicationHost.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private const string OptionsConfigurationRoot = "ApiOptions";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddValidatedOptions<ConnectionStringOptions>(Configuration, OptionsConfigurationRoot)
                .AddValidatedOptions<SecurityOptions>(Configuration, OptionsConfigurationRoot)
                .AddValidatedOptions<FeatureToggles>(Configuration, OptionsConfigurationRoot)
                .AddValidatedOptions<PublicKeyCacheOptions>(Configuration, OptionsConfigurationRoot)
                .AddValidatedOptions<TextCacheOptions>(Configuration, OptionsConfigurationRoot)
                .AddValidatedOptions<TextOptions>(Configuration, OptionsConfigurationRoot)
                .AddValidatedOptions<ThrottlingCircuitBreakerOptions>(Configuration, OptionsConfigurationRoot);

            services
                .AddControllers()
                .AddApplicationPart(typeof(Startup).Assembly)
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<ValidationAssemblyMarker>());

            services.AddMemoryCache();
            services.AddSingleton<ICacheManager, CacheManager>();
            services.AddApiVersioning(config =>
            {
                // Specify the default API Version
                config.DefaultApiVersion = new ApiVersion(1, 0);
                // If the client hasn't specified the API version in the request, use the default API version number
                config.AssumeDefaultVersionWhenUnspecified = true;
                // Advertise the API versions supported for the particular endpoint
                config.ReportApiVersions = true;
                config.ApiVersionReader = ApiVersionReader.Combine(
                        new UrlSegmentApiVersionReader(),
                        new HeaderApiVersionReader()
                        {
                            HeaderNames = { "api-version" }
                        },
                        new QueryStringApiVersionReader("v")
                );
            });

            services.AddConfiguredSwaggerGen();

            services.AddServiceDependencies();
            services.AddApplicationServices();
            services.AddScoped<ICertificatePublicKeyRepository, CertificatePublicKeyRepository>(); 

            services.AddHealthChecks()
                .AddCheck<PublicKeyServiceHealthCheck>("publickey", tags: new []{"publickey"})
                .AddCheck<TextServiceHealthCheck>("text", tags: new []{"text"});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandler("/error");
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FHICORC.ApplicationHost.Api v1"));

            app.UseRouting();               
            app.UseServiceMiddleware();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = HealthChecksOutput.WriteResponse
                });
                endpoints.MapHealthChecks("/publickeyhealth", new HealthCheckOptions
                {
                    Predicate = (check) => check.Tags.Contains("publickey"),
                    ResponseWriter = HealthChecksOutput.WriteResponse
                });
                endpoints.MapHealthChecks("/texthealth", new HealthCheckOptions
                {
                    Predicate = (check) => check.Tags.Contains("text"),
                    ResponseWriter = HealthChecksOutput.WriteResponse
                });
            });
        }
    }
}
