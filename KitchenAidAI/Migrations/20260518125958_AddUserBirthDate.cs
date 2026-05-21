using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitchenAidAI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBirthDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "godine",
                table: "Users");

            migrationBuilder.AddColumn<DateTime>(
                name: "datumRodenja",
                table: "Users",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "datumRodenja",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "godine",
                table: "Users",
                type: "int",
                nullable: true);
        }
    }
}
