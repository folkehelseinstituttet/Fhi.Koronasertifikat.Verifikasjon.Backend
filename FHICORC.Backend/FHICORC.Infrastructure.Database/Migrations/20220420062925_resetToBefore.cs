using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace FHICORC.Infrastructure.Database.Migrations
{
    public partial class resetToBefore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BatchesRevoc_FiltersRevoc_FiltersRevocBatchId",
                table: "BatchesRevoc");

            migrationBuilder.DropForeignKey(
                name: "FK_BatchesRevoc_HashesRevoc_HashesRevocId",
                table: "BatchesRevoc");

            migrationBuilder.DropIndex(
                name: "IX_BatchesRevoc_FiltersRevocBatchId",
                table: "BatchesRevoc");

            migrationBuilder.DropIndex(
                name: "IX_BatchesRevoc_HashesRevocId",
                table: "BatchesRevoc");

            migrationBuilder.DropColumn(
                name: "FiltersRevocBatchId",
                table: "BatchesRevoc");

            migrationBuilder.DropColumn(
                name: "HashesRevocId",
                table: "BatchesRevoc");

            migrationBuilder.AlterColumn<int>(
                name: "BatchId",
                table: "FiltersRevoc",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "IX_HashesRevoc_BatchId",
                table: "HashesRevoc",
                column: "BatchId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FiltersRevoc_BatchesRevoc_BatchId",
                table: "FiltersRevoc",
                column: "BatchId",
                principalTable: "BatchesRevoc",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HashesRevoc_BatchesRevoc_BatchId",
                table: "HashesRevoc",
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

            migrationBuilder.DropForeignKey(
                name: "FK_HashesRevoc_BatchesRevoc_BatchId",
                table: "HashesRevoc");

            migrationBuilder.DropIndex(
                name: "IX_HashesRevoc_BatchId",
                table: "HashesRevoc");

            migrationBuilder.AlterColumn<int>(
                name: "BatchId",
                table: "FiltersRevoc",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "FiltersRevocBatchId",
                table: "BatchesRevoc",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HashesRevocId",
                table: "BatchesRevoc",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BatchesRevoc_FiltersRevocBatchId",
                table: "BatchesRevoc",
                column: "FiltersRevocBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchesRevoc_HashesRevocId",
                table: "BatchesRevoc",
                column: "HashesRevocId");

            migrationBuilder.AddForeignKey(
                name: "FK_BatchesRevoc_FiltersRevoc_FiltersRevocBatchId",
                table: "BatchesRevoc",
                column: "FiltersRevocBatchId",
                principalTable: "FiltersRevoc",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BatchesRevoc_HashesRevoc_HashesRevocId",
                table: "BatchesRevoc",
                column: "HashesRevocId",
                principalTable: "HashesRevoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
