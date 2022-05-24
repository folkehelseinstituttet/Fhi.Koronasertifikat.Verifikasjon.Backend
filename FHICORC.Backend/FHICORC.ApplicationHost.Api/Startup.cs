using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
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
using FHICORC.Infrastructure.Database;
using FHICORC.Infrastructure.Database.Context;

namespace FHICORC.ApplicationHost.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddValidatedOptions<ConnectionStringOptions>(Configuration)
                .AddValidatedOptions<SecurityOptions>(Configuration)
                .AddValidatedOptions<FeatureToggles>(Configuration)
                .AddValidatedOptions<PublicKeyCacheOptions>(Configuration)
                .AddValidatedOptions<RuleCacheOptions>(Configuration)
                .AddValidatedOptions<TextCacheOptions>(Configuration)
                .AddValidatedOptions<TextOptions>(Configuration)
                .AddValidatedOptions<ValueSetCacheOptions>(Configuration)
                .AddValidatedOptions<ValueSetOptions>(Configuration);

            services
                .AddControllers()
                .AddApplicationPart(typeof(Startup).Assembly)
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<ValidationAssemblyMarker>());

            services.AddMemoryCache();
            services.AddSingleton<ICacheManager, CacheManager>();
            services.AddSingleton<IZipManager, ZipManager>();
            services.AddApiVersioning(config =>
            {
                // Specify the default API Version
                config.DefaultApiVersion = new ApiVersion(3, 0);
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
            services.AddVersionedApiExplorer(o =>
            {
                o.DefaultApiVersion = new ApiVersion(3, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.SubstituteApiVersionInUrl = true;
            });

            services.AddConfiguredSwaggerGen();

            var connectionStrings = Configuration.GetSection($"{nameof(ConnectionStringOptions)}").Get<ConnectionStringOptions>();
            services.AddDbContext<CoronapassContext>(options =>
                options.UseNpgsql(connectionStrings.PgsqlDatabase, b => b.MigrationsAssembly("FHICORC.Infrastructure.Database")));

            var featureToggles = Configuration.GetSection($"{nameof(FeatureToggles)}").Get<FeatureToggles>() ?? new();

            services.AddServiceDependencies();
            services.AddApplicationServices(featureToggles.UseEuDgcGateway);
            services.AddScoped<ICertificatePublicKeyRepository, CertificatePublicKeyRepository>();
            services.AddScoped<ICountriesReportRepository, CountriesReportRepository>();

            if (featureToggles.UseEuDgcGateway)
            {
                services.AddScoped<IEuCertificateRepository, EuCertificateRepository>();
            }

            services.AddScoped<IBusinessRuleRepository, BusinessRuleRepository>();

            services.AddHealthChecks()
                .AddDbContextCheck<CoronapassContext>()
                .AddCheck<PublicKeyServiceHealthCheck>("publickey", tags: new[] { "publickey" })
                .AddCheck<TextServiceHealthCheck>("text", tags: new[] { "text" })
                .AddCheck<RuleHealthCheck>("rule", tags: new[] { "rule" });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {

            if (env.IsDevelopment())
            {
                app.ApplicationServices.InitializeDatabase<CoronapassContext>();
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandler("/error");
            app.UseSwagger();

            // A common endpoint that contains both versions
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "swagger";
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Version {description.GroupName}");
                }
            });

            // Separate endpoints that contain only one version
            foreach (var description in provider.ApiVersionDescriptions)
            {
                app.UseSwaggerUI(c =>
                {
                    c.RoutePrefix = $"swagger/{description.GroupName}";
                    c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Version {description.GroupName}");
                });
                app.UseReDoc(c =>
                {
                    c.RoutePrefix = $"redoc/{description.GroupName}";
                    c.SpecUrl($"/swagger/{description.GroupName}/swagger.json");
                });
            }

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
                endpoints.MapHealthChecks("/rulehealth", new HealthCheckOptions
                {
                    Predicate = (check) => check.Tags.Contains("rule"),
                    ResponseWriter = HealthChecksOutput.WriteResponse
                });
            });
        }
    }
}
