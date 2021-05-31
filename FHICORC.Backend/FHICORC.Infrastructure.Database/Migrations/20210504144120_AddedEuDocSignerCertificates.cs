using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace FHICORC.Infrastructure.Database.Migrations
{
    public partial class AddedEuDocSignerCertificates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EuDocSignerCertificates",
                columns: table => new
                {
                    EuDocSignerCertificateId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KeyIdentifier = table.Column<string>(type: "varchar(50)", nullable: true),
                    Country = table.Column<string>(type: "varchar(10)", nullable: true),
                    CertificateType = table.Column<string>(type: "varchar(10)", nullable: true),
                    Thumbprint = table.Column<string>(type: "varchar(100)", nullable: true),
                    Signature = table.Column<string>(type: "text", nullable: true),
                    Certificate = table.Column<string>(type: "text", nullable: true),
                    PublicKey = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EuDocSignerCertificates", x => x.EuDocSignerCertificateId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EuDocSignerCertificates");
        }
    }
}
