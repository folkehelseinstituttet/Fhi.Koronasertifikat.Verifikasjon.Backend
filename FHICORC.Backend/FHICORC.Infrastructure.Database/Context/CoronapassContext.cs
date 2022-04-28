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
        public DbSet<BatchesRevoc> BatchesRevoc { get; set; }
        public DbSet<FiltersRevoc> FiltersRevoc { get; set; }
        public DbSet<HashesRevoc> HashesRevoc { get; set; }
        public DbSet<SuperFiltersRevoc> SuperFiltersRevoc { get; set; }

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


            //modelBuilder.Entity<HashesRevoc>()
            //     .HasOne(x => x.BatchesRevoc)
            //     .WithMany()
            //     .HasForeignKey(x => x.BatchId);


            //modelBuilder.Entity<BatchesRevoc>()
            //    .HasOne(a => a.FiltersRevoc)
            //    .WithOne(a => a.BatchesRevoc)
            //    .HasForeignKey<FiltersRevoc>(c => c.BatchId);


            //modelBuilder.Entity<BatchesRevoc>()
            // .HasOne(x => x.SuperFiltersRevoc)
            // .WithMany()
            // .HasForeignKey(x => x.SuperFiltersRevocId);


            base.OnModelCreating(modelBuilder);
        }
    }
}
