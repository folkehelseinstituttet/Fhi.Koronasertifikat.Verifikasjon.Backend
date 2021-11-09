using Microsoft.EntityFrameworkCore.Migrations;

namespace FHICORC.Infrastructure.Database.Migrations
{
    public partial class RulesJSON_20k_ColumnSize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RuleJson",
                table: "BusinessRules",
                type: "varchar(20000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10000)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RuleJson",
                table: "BusinessRules",
                type: "varchar(10000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20000)",
                oldNullable: true);
        }
    }
}
