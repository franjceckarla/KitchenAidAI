using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitchenAidAI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRegistrationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "godine",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ime",
                table: "Users",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "prezime",
                table: "Users",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "zemlja",
                table: "Users",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "godine",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "prezime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "zemlja",
                table: "Users");
        }
    }
}
