using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FHICORC.Application.Models.Options;
using FHICORC.Infrastructure.Database;
using FHICORC.Infrastructure.Database.Context;

namespace FHICORC.ApplicationHost.DbMigrations
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionStrings = Configuration.GetSection($"{nameof(ConnectionStringOptions)}").Get<ConnectionStringOptions>();

            services.AddDbContext<CoronapassContext>(options =>
                options.UseNpgsql(connectionStrings.PgsqlDatabase, b => b.MigrationsAssembly("FHICORC.Infrastructure.Database")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.ApplicationServices.InitializeDatabase<CoronapassContext>();
        }
    }
}
