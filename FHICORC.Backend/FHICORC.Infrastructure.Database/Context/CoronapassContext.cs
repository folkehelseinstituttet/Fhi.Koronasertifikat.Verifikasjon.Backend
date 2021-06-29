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
        public DbSet<BusinessRule> BusinessRules { get; set; }
		
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<EuDocSignerCertificate>()
                .Property(e => e.Created)
                .HasDefaultValueSql("now() at time zone 'utc'");

            modelBuilder
                .Entity<BusinessRule>()
                .Property(e => e.Created)
                .HasDefaultValueSql("now() at time zone 'utc'");

            modelBuilder.Entity<BusinessRule>()
                .HasIndex(r => r.RuleIdentifier)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
