using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddingPartProducedToWorkInstructions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CollectsProductSerialNumber",
                table: "WorkInstructions",
                newName: "PartProducedIsSerialized");

            migrationBuilder.AddColumn<int>(
                name: "PartProducedId",
                table: "WorkInstructions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkInstructions_PartProducedId",
                table: "WorkInstructions",
                column: "PartProducedId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkInstructions_PartDefinitions_PartProducedId",
                table: "WorkInstructions",
                column: "PartProducedId",
                principalTable: "PartDefinitions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkInstructions_PartDefinitions_PartProducedId",
                table: "WorkInstructions");

            migrationBuilder.DropIndex(
                name: "IX_WorkInstructions_PartProducedId",
                table: "WorkInstructions");

            migrationBuilder.DropColumn(
                name: "PartProducedId",
                table: "WorkInstructions");

            migrationBuilder.RenameColumn(
                name: "PartProducedIsSerialized",
                table: "WorkInstructions",
                newName: "CollectsProductSerialNumber");
        }
    }
}
