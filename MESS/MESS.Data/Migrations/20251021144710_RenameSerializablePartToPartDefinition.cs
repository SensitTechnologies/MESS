using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameSerializablePartToPartDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SerializableParts_PartDefinitions_PartId",
                table: "SerializableParts");

            migrationBuilder.RenameColumn(
                name: "PartId",
                table: "SerializableParts",
                newName: "PartDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_SerializableParts_PartId",
                table: "SerializableParts",
                newName: "IX_SerializableParts_PartDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SerializableParts_PartDefinitions_PartDefinitionId",
                table: "SerializableParts",
                column: "PartDefinitionId",
                principalTable: "PartDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SerializableParts_PartDefinitions_PartDefinitionId",
                table: "SerializableParts");

            migrationBuilder.RenameColumn(
                name: "PartDefinitionId",
                table: "SerializableParts",
                newName: "PartId");

            migrationBuilder.RenameIndex(
                name: "IX_SerializableParts_PartDefinitionId",
                table: "SerializableParts",
                newName: "IX_SerializableParts_PartId");

            migrationBuilder.AddForeignKey(
                name: "FK_SerializableParts_PartDefinitions_PartId",
                table: "SerializableParts",
                column: "PartId",
                principalTable: "PartDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
