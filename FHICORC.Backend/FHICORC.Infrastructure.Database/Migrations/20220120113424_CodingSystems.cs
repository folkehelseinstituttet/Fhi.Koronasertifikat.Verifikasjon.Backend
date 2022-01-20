using Microsoft.EntityFrameworkCore.Migrations;

namespace FHICORC.Infrastructure.Database.Migrations
{
    public partial class CodingSystems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VaccineCodesModels",
                columns: table => new
                {
                    VaccineCode = table.Column<string>(type: "varchar(5000)", nullable: false),
                    CodingSystem = table.Column<string>(type: "varchar(5000)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: true),
                    Manufacturer = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Type = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Target = table.Column<string>(type: "varchar(1000)", nullable: true),
                    IsAddManually = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaccineCodesModels", x => new { x.VaccineCode, x.CodingSystem });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VaccineCodesModels");
        }
    }
}
