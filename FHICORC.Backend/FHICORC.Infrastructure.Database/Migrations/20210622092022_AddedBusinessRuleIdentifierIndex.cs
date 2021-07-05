using Microsoft.EntityFrameworkCore.Migrations;

namespace FHICORC.Infrastructure.Database.Migrations
{
    public partial class AddedBusinessRuleIdentifierIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BusinessRules_RuleIdentifier",
                table: "BusinessRules",
                column: "RuleIdentifier",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BusinessRules_RuleIdentifier",
                table: "BusinessRules");
        }
    }
}
