using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FHICORC.Infrastructure.Database
{
    public static class InitializeDatabaseConfigureExtension
    {

        public static void InitializeDatabase<TDbContext>(this IServiceProvider applicationServices) where TDbContext : DbContext
        {
            using var scope = applicationServices.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            dbContext.Database.Migrate();
        }
    }
}
