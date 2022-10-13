using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace FHICORC.Infrastructure.Database.Migrations
{
    public partial class AddingRevocationTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RevocationDownloadJobSucceeded",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastDownloadJobSucceeded = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevocationDownloadJobSucceeded", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RevocationSuperFilter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SuperCountry = table.Column<string>(type: "text", nullable: true),
                    SuperExpires = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SuperFilter = table.Column<byte[]>(type: "bytea", maxLength: 5992, nullable: true),
                    BatchCount = table.Column<int>(type: "integer", nullable: false),
                    Bucket = table.Column<int>(type: "integer", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    HashType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevocationSuperFilter", x => x.Id);
                });

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
                    HashType = table.Column<int>(type: "integer", nullable: false),
                    Upload = table.Column<bool>(type: "boolean", nullable: false),
                    SuperId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevocationBatch", x => x.BatchId);
                    table.ForeignKey(
                        name: "FK_RevocationBatch_RevocationSuperFilter_SuperId",
                        column: x => x.SuperId,
                        principalTable: "RevocationSuperFilter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RevocationHash",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchId = table.Column<string>(type: "text", nullable: true),
                    Hash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevocationHash", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevocationHash_RevocationBatch_BatchId",
                        column: x => x.BatchId,
                        principalTable: "RevocationBatch",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RevocationBatch_SuperId",
                table: "RevocationBatch",
                column: "SuperId");

            migrationBuilder.CreateIndex(
                name: "IX_RevocationHash_BatchId",
                table: "RevocationHash",
                column: "BatchId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RevocationDownloadJobSucceeded");

            migrationBuilder.DropTable(
                name: "RevocationHash");

            migrationBuilder.DropTable(
                name: "RevocationBatch");

            migrationBuilder.DropTable(
                name: "RevocationSuperFilter");

        }
    }
}
