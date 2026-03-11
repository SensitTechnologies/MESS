using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class FailureNounsAndAdjectives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FailureAdjective",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailureAdjective", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FailureNoun",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailureNoun", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FailureNounAdjectives",
                columns: table => new
                {
                    AdjectivesId = table.Column<int>(type: "integer", nullable: false),
                    NounsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailureNounAdjectives", x => new { x.AdjectivesId, x.NounsId });
                    table.ForeignKey(
                        name: "FK_FailureNounAdjectives_FailureAdjective_AdjectivesId",
                        column: x => x.AdjectivesId,
                        principalTable: "FailureAdjective",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FailureNounAdjectives_FailureNoun_NounsId",
                        column: x => x.NounsId,
                        principalTable: "FailureNoun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FailureNounAdjectives_NounsId",
                table: "FailureNounAdjectives",
                column: "NounsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FailureNounAdjectives");

            migrationBuilder.DropTable(
                name: "FailureAdjective");

            migrationBuilder.DropTable(
                name: "FailureNoun");
        }
    }
}
