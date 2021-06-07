using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using FHICORC.Application.Common;

namespace FHICORC.ApplicationHost.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(options => options.AddEnvironmentVariables("FHICORC_"))
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder.UseStartup<Startup>().UseKestrel(opt => opt.AddServerHeader = false))
                .UseSerilogConfiguration("Api");
    }
}
