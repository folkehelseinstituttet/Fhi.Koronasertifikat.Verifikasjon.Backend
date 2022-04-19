using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace FHICORC.Infrastructure.Database.Migrations
{
    public partial class AllTheRevocTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchTableModels");

            migrationBuilder.DropTable(
                name: "BloomTableModels");

            migrationBuilder.CreateTable(
                name: "BatchesRevoc",
                columns: table => new
                {
                    BatchId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Expires = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Kid = table.Column<string>(type: "text", nullable: true),
                    HashType = table.Column<string>(type: "text", nullable: true),
                    Upload = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchesRevoc", x => x.BatchId);
                });

            migrationBuilder.CreateTable(
                name: "FiltersRevoc",
                columns: table => new
                {
                    BatchId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    BatchId = table.Column<int>(type: "integer", nullable: false),
                    Hash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HashesRevoc", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchesRevoc");

            migrationBuilder.DropTable(
                name: "FiltersRevoc");

            migrationBuilder.DropTable(
                name: "HashesRevoc");

            migrationBuilder.CreateTable(
                name: "BatchTableModels",
                columns: table => new
                {
                    BatchId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Country = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Expires = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    HashType = table.Column<string>(type: "text", nullable: true),
                    Kid = table.Column<string>(type: "text", nullable: true),
                    Upload = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchTableModels", x => x.BatchId);
                });

            migrationBuilder.CreateTable(
                name: "BloomTableModels",
                columns: table => new
                {
                    BatchId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Filter = table.Column<byte[]>(type: "bytea", maxLength: 47926, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloomTableModels", x => x.BatchId);
                });
        }
    }
}
