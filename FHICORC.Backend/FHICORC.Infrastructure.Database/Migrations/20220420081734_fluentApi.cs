using Microsoft.EntityFrameworkCore.Migrations;

namespace FHICORC.Infrastructure.Database.Migrations
{
    public partial class fluentApi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HashesRevoc_BatchId",
                table: "HashesRevoc");

            migrationBuilder.AddColumn<int>(
                name: "HashesRevocId",
                table: "BatchesRevoc",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HashesRevoc_BatchId",
                table: "HashesRevoc",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchesRevoc_HashesRevocId",
                table: "BatchesRevoc",
                column: "HashesRevocId");

            migrationBuilder.AddForeignKey(
                name: "FK_BatchesRevoc_HashesRevoc_HashesRevocId",
                table: "BatchesRevoc",
                column: "HashesRevocId",
                principalTable: "HashesRevoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BatchesRevoc_HashesRevoc_HashesRevocId",
                table: "BatchesRevoc");

            migrationBuilder.DropIndex(
                name: "IX_HashesRevoc_BatchId",
                table: "HashesRevoc");

            migrationBuilder.DropIndex(
                name: "IX_BatchesRevoc_HashesRevocId",
                table: "BatchesRevoc");

            migrationBuilder.DropColumn(
                name: "HashesRevocId",
                table: "BatchesRevoc");

            migrationBuilder.CreateIndex(
                name: "IX_HashesRevoc_BatchId",
                table: "HashesRevoc",
                column: "BatchId",
                unique: true);
        }
    }
}
