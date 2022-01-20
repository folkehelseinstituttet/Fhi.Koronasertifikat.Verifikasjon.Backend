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
        public DbSet<CountriesReportModel> CountriesReportModels { get; set; }
        public DbSet<TrustedIssuerModel> TrustedIssuerModels { get; set; }
        public DbSet<VaccineCodesModel> VaccineCodesModels { get; set; }

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

            modelBuilder
                .Entity<VaccineCodesModel>()
                .HasKey(k => new {k.VaccineCode, k.CodingSystem});
            
            modelBuilder
                .Entity<VaccineCodesModel>()
                .Property(k => k.IsAddManually).HasDefaultValue(true);

            base.OnModelCreating(modelBuilder);
        }
    }
}
