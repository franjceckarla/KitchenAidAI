using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitchenAidAI.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "kreirano",
                table: "ReceptKuharice",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "kreirano",
                table: "Namirnice",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "kreirano",
                table: "KoraciRecepta",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "kreirano",
                table: "Frizideri",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "kreirano",
                table: "ReceptKuharice");

            migrationBuilder.DropColumn(
                name: "kreirano",
                table: "Namirnice");

            migrationBuilder.DropColumn(
                name: "kreirano",
                table: "KoraciRecepta");

            migrationBuilder.DropColumn(
                name: "kreirano",
                table: "Frizideri");
        }
    }
}
