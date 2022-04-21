using Microsoft.EntityFrameworkCore.Migrations;

namespace FHICORC.Infrastructure.Database.Migrations
{
    public partial class RemovedFKConstrains : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BatchesRevoc_SuperFiltersRevoc_SuperFiltersRevocId",
                table: "BatchesRevoc");

            migrationBuilder.DropIndex(
                name: "IX_BatchesRevoc_SuperFiltersRevocId",
                table: "BatchesRevoc");

            migrationBuilder.AlterColumn<int>(
                name: "SuperFiltersRevocId",
                table: "BatchesRevoc",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SuperFiltersRevocId",
                table: "BatchesRevoc",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BatchesRevoc_SuperFiltersRevocId",
                table: "BatchesRevoc",
                column: "SuperFiltersRevocId");

            migrationBuilder.AddForeignKey(
                name: "FK_BatchesRevoc_SuperFiltersRevoc_SuperFiltersRevocId",
                table: "BatchesRevoc",
                column: "SuperFiltersRevocId",
                principalTable: "SuperFiltersRevoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
