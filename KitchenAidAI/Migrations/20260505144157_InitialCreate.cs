using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitchenAidAI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Recepti",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    naziv = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    opis = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    vrijemeKuhanja = table.Column<double>(type: "double", nullable: false),
                    tezina = table.Column<int>(type: "int", nullable: false),
                    brojPorcija = table.Column<int>(type: "int", nullable: false),
                    datumKreiranja = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recepti", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    username = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    kreirano = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    preferencijaPrehrane = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "KoraciRecepta",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    receptId = table.Column<int>(type: "int", nullable: false),
                    redniBroj = table.Column<int>(type: "int", nullable: false),
                    opis = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trajanje = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KoraciRecepta", x => x.id);
                    table.ForeignKey(
                        name: "FK_KoraciRecepta_Recepti_receptId",
                        column: x => x.receptId,
                        principalTable: "Recepti",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    userId = table.Column<int>(type: "int", nullable: false),
                    message = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    response = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    datumKreiranja = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    tip = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Frizideri",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    userId = table.Column<int>(type: "int", nullable: true),
                    azurirano = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frizideri", x => x.id);
                    table.ForeignKey(
                        name: "FK_Frizideri_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Kuharice",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    naziv = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    userId = table.Column<int>(type: "int", nullable: true),
                    kreirano = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kuharice", x => x.id);
                    table.ForeignKey(
                        name: "FK_Kuharice_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Namirnice",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    friziderId = table.Column<int>(type: "int", nullable: false),
                    naziv = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    kategorija = table.Column<int>(type: "int", nullable: false),
                    nutritivnaVrijednost_kalorije = table.Column<double>(type: "double", nullable: true),
                    nutritivnaVrijednost_proteini = table.Column<double>(type: "double", nullable: true),
                    nutritivnaVrijednost_ugljikohidrati = table.Column<double>(type: "double", nullable: true),
                    nutritivnaVrijednost_masti = table.Column<double>(type: "double", nullable: true),
                    nutritivnaVrijednost_vlakna = table.Column<double>(type: "double", nullable: true),
                    nutritivnaVrijednost_sol = table.Column<double>(type: "double", nullable: true),
                    mjera = table.Column<int>(type: "int", nullable: false),
                    kolicinaUFrizideru = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Namirnice", x => x.id);
                    table.ForeignKey(
                        name: "FK_Namirnice_Frizideri_friziderId",
                        column: x => x.friziderId,
                        principalTable: "Frizideri",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReceptKuharice",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    receptId = table.Column<int>(type: "int", nullable: false),
                    kuharicaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceptKuharice", x => x.id);
                    table.ForeignKey(
                        name: "FK_ReceptKuharice_Kuharice_kuharicaId",
                        column: x => x.kuharicaId,
                        principalTable: "Kuharice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceptKuharice_Recepti_receptId",
                        column: x => x.receptId,
                        principalTable: "Recepti",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_userId",
                table: "ChatMessages",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_Frizideri_userId",
                table: "Frizideri",
                column: "userId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KoraciRecepta_receptId",
                table: "KoraciRecepta",
                column: "receptId");

            migrationBuilder.CreateIndex(
                name: "IX_Kuharice_userId",
                table: "Kuharice",
                column: "userId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Namirnice_friziderId",
                table: "Namirnice",
                column: "friziderId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceptKuharice_kuharicaId",
                table: "ReceptKuharice",
                column: "kuharicaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceptKuharice_receptId_kuharicaId",
                table: "ReceptKuharice",
                columns: new[] { "receptId", "kuharicaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "KoraciRecepta");

            migrationBuilder.DropTable(
                name: "Namirnice");

            migrationBuilder.DropTable(
                name: "ReceptKuharice");

            migrationBuilder.DropTable(
                name: "Frizideri");

            migrationBuilder.DropTable(
                name: "Kuharice");

            migrationBuilder.DropTable(
                name: "Recepti");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
