using Microsoft.EntityFrameworkCore.Migrations;

namespace FHICORC.Infrastructure.Database.Migrations
{
    public partial class VirtualFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_HashesRevoc_BatchId",
                table: "HashesRevoc",
                column: "BatchId",
                unique: true);

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
                name: "FK_HashesRevoc_BatchesRevoc_BatchId",
                table: "HashesRevoc");

            migrationBuilder.DropIndex(
                name: "IX_HashesRevoc_BatchId",
                table: "HashesRevoc");
        }
    }
}
