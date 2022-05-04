using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FHICORC.Infrastructure.Database.Migrations
{
    public partial class nameChangeRevocationBatch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FiltersRevoc_BatchesRevoc_BatchId",
                table: "FiltersRevoc");

            migrationBuilder.DropForeignKey(
                name: "FK_HashesRevoc_BatchesRevoc_BatchId",
                table: "HashesRevoc");

            migrationBuilder.DropTable(
                name: "BatchesRevoc");

            migrationBuilder.CreateTable(
                name: "RevocationBatch",
                columns: table => new
                {
                    BatchId = table.Column<string>(type: "text", nullable: false),
                    Expires = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Kid = table.Column<string>(type: "text", nullable: true),
                    HashType = table.Column<string>(type: "text", nullable: true),
                    Upload = table.Column<bool>(type: "boolean", nullable: false),
                    SuperId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevocationBatch", x => x.BatchId);
                    table.ForeignKey(
                        name: "FK_RevocationBatch_SuperFiltersRevoc_SuperId",
                        column: x => x.SuperId,
                        principalTable: "SuperFiltersRevoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RevocationBatch_SuperId",
                table: "RevocationBatch",
                column: "SuperId");

            migrationBuilder.AddForeignKey(
                name: "FK_FiltersRevoc_RevocationBatch_BatchId",
                table: "FiltersRevoc",
                column: "BatchId",
                principalTable: "RevocationBatch",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HashesRevoc_RevocationBatch_BatchId",
                table: "HashesRevoc",
                column: "BatchId",
                principalTable: "RevocationBatch",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FiltersRevoc_RevocationBatch_BatchId",
                table: "FiltersRevoc");

            migrationBuilder.DropForeignKey(
                name: "FK_HashesRevoc_RevocationBatch_BatchId",
                table: "HashesRevoc");

            migrationBuilder.DropTable(
                name: "RevocationBatch");

            migrationBuilder.CreateTable(
                name: "BatchesRevoc",
                columns: table => new
                {
                    BatchId = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Expires = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    HashType = table.Column<string>(type: "text", nullable: true),
                    Kid = table.Column<string>(type: "text", nullable: true),
                    SuperId = table.Column<int>(type: "integer", nullable: true),
                    Upload = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchesRevoc", x => x.BatchId);
                    table.ForeignKey(
                        name: "FK_BatchesRevoc_SuperFiltersRevoc_SuperId",
                        column: x => x.SuperId,
                        principalTable: "SuperFiltersRevoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatchesRevoc_SuperId",
                table: "BatchesRevoc",
                column: "SuperId");

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
                onDelete: ReferentialAction.Restrict);
        }
    }
}
