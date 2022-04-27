using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FHICORC.Infrastructure.Database.Migrations
{
    public partial class RevocModifiedDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Changed",
                table: "SuperFiltersRevoc");

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "SuperFiltersRevoc",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Modified",
                table: "SuperFiltersRevoc");

            migrationBuilder.AddColumn<bool>(
                name: "Changed",
                table: "SuperFiltersRevoc",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
