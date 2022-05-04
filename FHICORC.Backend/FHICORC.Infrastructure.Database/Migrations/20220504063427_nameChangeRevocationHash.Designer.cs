﻿// <auto-generated />
using System;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace FHICORC.Infrastructure.Database.Migrations
{
    [DbContext(typeof(CoronapassContext))]
    [Migration("20220504063427_nameChangeRevocationHash")]
    partial class nameChangeRevocationHash
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.6")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("FHICORC.Domain.Models.BusinessRule", b =>
                {
                    b.Property<int>("BusinessRuleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("now() at time zone 'utc'");

                    b.Property<string>("RuleIdentifier")
                        .HasColumnType("varchar(20)");

                    b.Property<string>("RuleJson")
                        .HasColumnType("varchar(20000)");

                    b.HasKey("BusinessRuleId");

                    b.HasIndex("RuleIdentifier")
                        .IsUnique();

                    b.ToTable("BusinessRules");
                });

            modelBuilder.Entity("FHICORC.Domain.Models.CountriesReportModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("CountriesReport")
                        .HasColumnType("varchar(5000)");

                    b.HasKey("Id");

                    b.ToTable("CountriesReportModels");
                });

            modelBuilder.Entity("FHICORC.Domain.Models.EuDocSignerCertificate", b =>
                {
                    b.Property<int>("EuDocSignerCertificateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Certificate")
                        .HasColumnType("text");

                    b.Property<string>("CertificateType")
                        .HasColumnType("varchar(10)");

                    b.Property<string>("Country")
                        .HasColumnType("varchar(10)");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("now() at time zone 'utc'");

                    b.Property<string>("KeyIdentifier")
                        .HasColumnType("varchar(50)");

                    b.Property<string>("PublicKey")
                        .HasColumnType("text");

                    b.Property<string>("Signature")
                        .HasColumnType("text");

                    b.Property<string>("Thumbprint")
                        .HasColumnType("varchar(100)");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("EuDocSignerCertificateId");

                    b.ToTable("EuDocSignerCertificates");
                });

            modelBuilder.Entity("FHICORC.Domain.Models.FiltersRevoc", b =>
                {
                    b.Property<string>("BatchId")
                        .HasColumnType("text");

                    b.Property<byte[]>("Filter")
                        .HasMaxLength(5992)
                        .HasColumnType("bytea");

                    b.HasKey("BatchId");

                    b.ToTable("FiltersRevoc");
                });

            modelBuilder.Entity("FHICORC.Domain.Models.RevocationBatch", b =>
                {
                    b.Property<string>("BatchId")
                        .HasColumnType("text");

                    b.Property<string>("Country")
                        .HasColumnType("text");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("Deleted")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("Expires")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("HashType")
                        .HasColumnType("text");

                    b.Property<string>("Kid")
                        .HasColumnType("text");

                    b.Property<int?>("SuperId")
                        .HasColumnType("integer");

                    b.Property<bool>("Upload")
                        .HasColumnType("boolean");

                    b.HasKey("BatchId");

                    b.HasIndex("SuperId");

                    b.ToTable("RevocationBatch");
                });

            modelBuilder.Entity("FHICORC.Domain.Models.RevocationHash", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("BatchId")
                        .HasColumnType("text");

                    b.Property<string>("Hash")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.ToTable("RevocationHash");
                });

            modelBuilder.Entity("FHICORC.Domain.Models.SuperFiltersRevoc", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("BatchCount")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("SuperExpires")
                        .HasColumnType("timestamp without time zone");

                    b.Property<byte[]>("SuperFilter")
                        .HasMaxLength(5992)
                        .HasColumnType("bytea");

                    b.HasKey("Id");

                    b.ToTable("SuperFiltersRevoc");
                });

            modelBuilder.Entity("FHICORC.Domain.Models.FiltersRevoc", b =>
                {
                    b.HasOne("FHICORC.Domain.Models.RevocationBatch", "RevocationBatch")
                        .WithOne("FiltersRevoc")
                        .HasForeignKey("FHICORC.Domain.Models.FiltersRevoc", "BatchId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("RevocationBatch");
                });

            modelBuilder.Entity("FHICORC.Domain.Models.RevocationBatch", b =>
                {
                    b.HasOne("FHICORC.Domain.Models.SuperFiltersRevoc", "SuperFiltersRevoc")
                        .WithMany("RevocationBatches")
                        .HasForeignKey("SuperId");

                    b.Navigation("SuperFiltersRevoc");
                });

            modelBuilder.Entity("FHICORC.Domain.Models.RevocationHash", b =>
                {
                    b.HasOne("FHICORC.Domain.Models.RevocationBatch", "RevocationBatch")
                        .WithMany("RevocationHashes")
                        .HasForeignKey("BatchId");

                    b.Navigation("RevocationBatch");
                });

            modelBuilder.Entity("FHICORC.Domain.Models.RevocationBatch", b =>
                {
                    b.Navigation("FiltersRevoc");

                    b.Navigation("RevocationHashes");
                });

            modelBuilder.Entity("FHICORC.Domain.Models.SuperFiltersRevoc", b =>
                {
                    b.Navigation("RevocationBatches");
                });
#pragma warning restore 612, 618
        }
    }
}
