using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace FHICORC.Infrastructure.Database.Migrations
{
    public partial class nameChangeRevocationHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HashesRevoc");

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
                name: "IX_RevocationHash_BatchId",
                table: "RevocationHash",
                column: "BatchId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RevocationHash");

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
                    table.ForeignKey(
                        name: "FK_HashesRevoc_RevocationBatch_BatchId",
                        column: x => x.BatchId,
                        principalTable: "RevocationBatch",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HashesRevoc_BatchId",
                table: "HashesRevoc",
                column: "BatchId");
        }
    }
}
