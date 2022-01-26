using Microsoft.EntityFrameworkCore.Migrations;

namespace FHICORC.Infrastructure.Database.Migrations
{
    public partial class TrustedIssuer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddPrimaryKey(
                name: "PK_CountriesReportModels",
                table: "CountriesReportModels",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "TrustedIssuerModels",
                columns: table => new
                {
                    Iss = table.Column<string>(type: "varchar(5000)", nullable: false),
                    Name = table.Column<string>(type: "varchar(5000)", nullable: true),
                    IsAddManually = table.Column<bool>(type: "boolean", nullable: false),
                    IsMarkedUntrusted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustedIssuerModels", x => x.Iss);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrustedIssuerModels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CountriesReportModels",
                table: "CountriesReportModels");
        }
    }
}
