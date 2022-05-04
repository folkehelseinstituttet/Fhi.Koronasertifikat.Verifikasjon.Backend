using Microsoft.EntityFrameworkCore.Migrations;

namespace FHICORC.Infrastructure.Database.Migrations
{
    public partial class ChangedBatchesToFiltersRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FiltersRevoc_BatchesRevoc_BatchesRevocBatchId",
                table: "FiltersRevoc");

            migrationBuilder.DropIndex(
                name: "IX_FiltersRevoc_BatchesRevocBatchId",
                table: "FiltersRevoc");

            migrationBuilder.DropColumn(
                name: "BatchesRevocBatchId",
                table: "FiltersRevoc");

            migrationBuilder.AddForeignKey(
                name: "FK_FiltersRevoc_BatchesRevoc_BatchId",
                table: "FiltersRevoc",
                column: "BatchId",
                principalTable: "BatchesRevoc",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FiltersRevoc_BatchesRevoc_BatchId",
                table: "FiltersRevoc");

            migrationBuilder.AddColumn<string>(
                name: "BatchesRevocBatchId",
                table: "FiltersRevoc",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FiltersRevoc_BatchesRevocBatchId",
                table: "FiltersRevoc",
                column: "BatchesRevocBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_FiltersRevoc_BatchesRevoc_BatchesRevocBatchId",
                table: "FiltersRevoc",
                column: "BatchesRevocBatchId",
                principalTable: "BatchesRevoc",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
