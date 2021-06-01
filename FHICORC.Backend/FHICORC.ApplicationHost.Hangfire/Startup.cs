using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FHICORC.Application.Common;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Repositories;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Application.Validation;
using FHICORC.Infrastructure.Database;
using FHICORC.Infrastructure.Database.Context;
using System;
using HealthChecks.Hangfire;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using FHICORC.ApplicationHost.Hangfire.Interfaces;
using FHICORC.Integrations.DGCGateway;

namespace FHICORC.ApplicationHost.Hangfire
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container. 
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddValidatedOptions<ConnectionStringOptions>(Configuration)
                .AddValidatedOptions<CronOptions>(Configuration)
                .AddValidatedOptions<CertificateOptions>(Configuration)
                .AddValidatedOptions<ServiceEndpoints>(Configuration)
                .AddValidatedOptions<FeatureToggles>(Configuration)
                .AddValidatedOptions<HangfireHealthOptions>(Configuration);

            services
                .AddControllers()
                .AddApplicationPart(typeof(Startup).Assembly)
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<ValidationAssemblyMarker>());

            var connectionStrings = Configuration.GetSection($"{nameof(ConnectionStringOptions)}").Get<ConnectionStringOptions>();
            services.AddDbContext<HangfireContext>(options =>
                options.UseNpgsql(connectionStrings.HangfirePgsqlDatabase));

            services.AddDbContext<CoronapassContext>(options =>
                options.UseNpgsql(connectionStrings.PgsqlDatabase,
                    b => b.MigrationsAssembly("FHICORC.Infrastructure.Database")));
            services.AddScoped<IEuCertificateRepository, EuCertificateRepository>();

            services.AddHangfire(configuration => configuration.UsePostgreSqlStorage(connectionStrings.HangfirePgsqlDatabase));
            services.AddHangfireTaskManagerAndTasks();

            services.AddDgcgGatewayIntegration();

            HangfireHealthOptions hangfireHealthOptions = Configuration.GetSection($"{nameof(HangfireHealthOptions)}").Get<HangfireHealthOptions>();
            services.AddHealthChecks()
                .AddDbContextCheck<CoronapassContext>()
                .AddDbContextCheck<HangfireContext>()
                .AddHangfire(_ => new HangfireOptions
                {
                    MaximumJobsFailed = hangfireHealthOptions.MaximumJobsFailed,
                    MinimumAvailableServers = hangfireHealthOptions.MinimumAvailableServers
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IHangfireTaskManager hangfireTaskManager)
        {
            if (env.IsDevelopment())
            {
                app.ApplicationServices.InitializeDatabase<CoronapassContext>();
                app.UseDeveloperExceptionPage();
            }

            InitializeHangfireDatabase(app.ApplicationServices);
            app.UseHangfireDashboard();
            app.UseHangfireServer();
            hangfireTaskManager.SetupHangfireTasks();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions{
                    ResponseWriter = HealthChecksOutput.WriteResponse
                });
            });
        }

        private static void InitializeHangfireDatabase(IServiceProvider applicationServices)
        {
            using var scope = applicationServices.CreateScope();
            using var hangfireContext = scope.ServiceProvider.GetRequiredService<HangfireContext>();
            hangfireContext.Database.Migrate();
        }
    }
}
