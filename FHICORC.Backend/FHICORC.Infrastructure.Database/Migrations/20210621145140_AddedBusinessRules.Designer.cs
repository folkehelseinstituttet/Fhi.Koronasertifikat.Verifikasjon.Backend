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
    [Migration("20210621145140_AddedBusinessRules")]
    partial class AddedBusinessRules
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
                        .HasColumnType("varchar(10000)");

                    b.HasKey("BusinessRuleId");

                    b.ToTable("BusinessRules");
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
#pragma warning restore 612, 618
        }
    }
}
