using Microsoft.EntityFrameworkCore;
using FHICORC.Domain.Models;

namespace FHICORC.Infrastructure.Database.Context
{
    public class CoronapassContext : DbContext
    {
        public CoronapassContext(DbContextOptions<CoronapassContext> options)
            : base(options)
        {
        }

        public DbSet<EuDocSignerCertificate> EuDocSignerCertificates { get; set; }
		
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<EuDocSignerCertificate>()
                .Property(e => e.Created)
                .HasDefaultValueSql("now() at time zone 'utc'");

            base.OnModelCreating(modelBuilder);
        }
    }
}
