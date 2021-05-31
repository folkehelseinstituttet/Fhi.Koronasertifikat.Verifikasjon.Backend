using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using FHICORC.Application.Common;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace FHICORC.ApplicationHost.DbMigrations
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            CreateHostBuilder(args).Build().RunAsync(cancellationTokenSource.Token);
            cancellationTokenSource.Cancel();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(options => options.AddEnvironmentVariables("FHICORC_"))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseConsoleLifetime()
                .UseSerilogConfiguration("DbMigrations");
    }
}
