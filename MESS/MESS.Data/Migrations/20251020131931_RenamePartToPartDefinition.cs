using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenamePartToPartDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename the Parts table to PartDefinitions
            migrationBuilder.RenameTable(
                name: "Parts",
                newName: "PartDefinitions");

            // Rename columns inside the renamed table
            migrationBuilder.RenameColumn(
                name: "PartNumber",
                table: "PartDefinitions",
                newName: "Number");

            migrationBuilder.RenameColumn(
                name: "PartName",
                table: "PartDefinitions",
                newName: "Name");

            // Drop old foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_PartNodeParts_Parts_PartsId",
                table: "PartNodeParts");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogParts_Parts_PartId",
                table: "ProductionLogParts");

            migrationBuilder.DropForeignKey(
                name: "FK_SerializableParts_Parts_PartId",
                table: "SerializableParts");

            // Add new foreign keys pointing to the renamed table
            migrationBuilder.AddForeignKey(
                name: "FK_PartNodeParts_PartDefinitions_PartsId",
                table: "PartNodeParts",
                column: "PartsId",
                principalTable: "PartDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogParts_PartDefinitions_PartId",
                table: "ProductionLogParts",
                column: "PartId",
                principalTable: "PartDefinitions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SerializableParts_PartDefinitions_PartId",
                table: "SerializableParts",
                column: "PartId",
                principalTable: "PartDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop new foreign keys first
            migrationBuilder.DropForeignKey(
                name: "FK_PartNodeParts_PartDefinitions_PartsId",
                table: "PartNodeParts");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogParts_PartDefinitions_PartId",
                table: "ProductionLogParts");

            migrationBuilder.DropForeignKey(
                name: "FK_SerializableParts_PartDefinitions_PartId",
                table: "SerializableParts");

            // Rename table back
            migrationBuilder.RenameTable(
                name: "PartDefinitions",
                newName: "Parts");

            // Rename columns back
            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Parts",
                newName: "PartNumber");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Parts",
                newName: "PartName");

            // Restore foreign keys
            migrationBuilder.AddForeignKey(
                name: "FK_PartNodeParts_Parts_PartsId",
                table: "PartNodeParts",
                column: "PartsId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogParts_Parts_PartId",
                table: "ProductionLogParts",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SerializableParts_Parts_PartId",
                table: "SerializableParts",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}