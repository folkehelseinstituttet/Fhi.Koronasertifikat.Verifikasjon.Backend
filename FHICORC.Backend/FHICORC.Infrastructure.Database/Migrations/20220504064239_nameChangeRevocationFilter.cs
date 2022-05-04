using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace FHICORC.Infrastructure.Database.Migrations
{
    public partial class nameChangeRevocationFilter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RevocationBatch_SuperFiltersRevoc_SuperId",
                table: "RevocationBatch");

            migrationBuilder.DropTable(
                name: "FiltersRevoc");

            migrationBuilder.DropTable(
                name: "SuperFiltersRevoc");

            migrationBuilder.CreateTable(
                name: "RevocationFilter",
                columns: table => new
                {
                    BatchId = table.Column<string>(type: "text", nullable: false),
                    Filter = table.Column<byte[]>(type: "bytea", maxLength: 5992, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevocationFilter", x => x.BatchId);
                    table.ForeignKey(
                        name: "FK_RevocationFilter_RevocationBatch_BatchId",
                        column: x => x.BatchId,
                        principalTable: "RevocationBatch",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RevocationSuperFilter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SuperExpires = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SuperFilter = table.Column<byte[]>(type: "bytea", maxLength: 5992, nullable: true),
                    BatchCount = table.Column<int>(type: "integer", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevocationSuperFilter", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_RevocationBatch_RevocationSuperFilter_SuperId",
                table: "RevocationBatch",
                column: "SuperId",
                principalTable: "RevocationSuperFilter",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RevocationBatch_RevocationSuperFilter_SuperId",
                table: "RevocationBatch");

            migrationBuilder.DropTable(
                name: "RevocationFilter");

            migrationBuilder.DropTable(
                name: "RevocationSuperFilter");

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
                    table.ForeignKey(
                        name: "FK_FiltersRevoc_RevocationBatch_BatchId",
                        column: x => x.BatchId,
                        principalTable: "RevocationBatch",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SuperFiltersRevoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchCount = table.Column<int>(type: "integer", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SuperExpires = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SuperFilter = table.Column<byte[]>(type: "bytea", maxLength: 5992, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuperFiltersRevoc", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_RevocationBatch_SuperFiltersRevoc_SuperId",
                table: "RevocationBatch",
                column: "SuperId",
                principalTable: "SuperFiltersRevoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
