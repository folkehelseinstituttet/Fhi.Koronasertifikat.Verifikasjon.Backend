using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace FHICORC.Infrastructure.Database.Migrations
{
    public partial class RevocDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddPrimaryKey(
                name: "PK_CountriesReportModels",
                table: "CountriesReportModels",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SuperFiltersRevoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SuperExpires = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SuperFilter = table.Column<byte[]>(type: "bytea", maxLength: 5992, nullable: true),
                    Changed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuperFiltersRevoc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FiltersRevoc",
                columns: table => new
                {
                    BatchId = table.Column<string>(type: "text", nullable: false),
                    Filter = table.Column<byte[]>(type: "bytea", maxLength: 5992, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiltersRevoc", x => x.BatchId);
                });

            migrationBuilder.CreateTable(
                name: "HashesRevoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchId = table.Column<string>(type: "text", nullable: true),
                    Hash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HashesRevoc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BatchesRevoc",
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
                    SuperFiltersRevocId = table.Column<int>(type: "integer", nullable: true),
                    HashesRevocId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchesRevoc", x => x.BatchId);
                    table.ForeignKey(
                        name: "FK_BatchesRevoc_HashesRevoc_HashesRevocId",
                        column: x => x.HashesRevocId,
                        principalTable: "HashesRevoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatchesRevoc_HashesRevocId",
                table: "BatchesRevoc",
                column: "HashesRevocId");

            migrationBuilder.CreateIndex(
                name: "IX_HashesRevoc_BatchId",
                table: "HashesRevoc",
                column: "BatchId");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BatchesRevoc_HashesRevoc_HashesRevocId",
                table: "BatchesRevoc");

            migrationBuilder.DropTable(
                name: "FiltersRevoc");

            migrationBuilder.DropTable(
                name: "SuperFiltersRevoc");

            migrationBuilder.DropTable(
                name: "HashesRevoc");

            migrationBuilder.DropTable(
                name: "BatchesRevoc");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CountriesReportModels",
                table: "CountriesReportModels");
        }
    }
}
