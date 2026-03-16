using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovingParentPartFromProductionLogPart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogParts_SerializableParts_ParentPartId",
                table: "ProductionLogParts");

            migrationBuilder.DropIndex(
                name: "IX_ProductionLogParts_ParentPartId",
                table: "ProductionLogParts");

            migrationBuilder.DropColumn(
                name: "ParentPartId",
                table: "ProductionLogParts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentPartId",
                table: "ProductionLogParts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogParts_ParentPartId",
                table: "ProductionLogParts",
                column: "ParentPartId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogParts_SerializableParts_ParentPartId",
                table: "ProductionLogParts",
                column: "ParentPartId",
                principalTable: "SerializableParts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
