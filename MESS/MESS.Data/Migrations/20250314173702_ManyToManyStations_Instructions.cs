using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class ManyToManyStations_Instructions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkInstructions_Products_ProductId",
                table: "WorkInstructions");

            migrationBuilder.DropIndex(
                name: "IX_WorkInstructions_ProductId",
                table: "WorkInstructions");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "WorkInstructions");

            migrationBuilder.CreateTable(
                name: "ProductWorkInstruction",
                columns: table => new
                {
                    ProductsId = table.Column<int>(type: "int", nullable: false),
                    WorkInstructionsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductWorkInstruction", x => new { x.ProductsId, x.WorkInstructionsId });
                    table.ForeignKey(
                        name: "FK_ProductWorkInstruction_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductWorkInstruction_WorkInstructions_WorkInstructionsId",
                        column: x => x.WorkInstructionsId,
                        principalTable: "WorkInstructions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkInstructionWorkStation",
                columns: table => new
                {
                    WorkInstructionsId = table.Column<int>(type: "int", nullable: false),
                    WorkStationsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkInstructionWorkStation", x => new { x.WorkInstructionsId, x.WorkStationsId });
                    table.ForeignKey(
                        name: "FK_WorkInstructionWorkStation_WorkInstructions_WorkInstructionsId",
                        column: x => x.WorkInstructionsId,
                        principalTable: "WorkInstructions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkInstructionWorkStation_WorkStations_WorkStationsId",
                        column: x => x.WorkStationsId,
                        principalTable: "WorkStations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductWorkInstruction_WorkInstructionsId",
                table: "ProductWorkInstruction",
                column: "WorkInstructionsId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkInstructionWorkStation_WorkStationsId",
                table: "WorkInstructionWorkStation",
                column: "WorkStationsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductWorkInstruction");

            migrationBuilder.DropTable(
                name: "WorkInstructionWorkStation");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "WorkInstructions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkInstructions_ProductId",
                table: "WorkInstructions",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkInstructions_Products_ProductId",
                table: "WorkInstructions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
