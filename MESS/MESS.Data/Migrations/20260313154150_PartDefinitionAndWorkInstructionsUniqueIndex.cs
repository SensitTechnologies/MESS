using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class PartDefinitionAndWorkInstructionsUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WorkInstructions_Title_Version",
                table: "WorkInstructions",
                columns: new[] { "Title", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartDefinitions_Name",
                table: "PartDefinitions",
                column: "Name",
                unique: true,
                filter: "\"Number\" IS NULL OR \"Number\" = ''");

            migrationBuilder.CreateIndex(
                name: "IX_PartDefinitions_Name_Number",
                table: "PartDefinitions",
                columns: new[] { "Name", "Number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkInstructions_Title_Version",
                table: "WorkInstructions");

            migrationBuilder.DropIndex(
                name: "IX_PartDefinitions_Name",
                table: "PartDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_PartDefinitions_Name_Number",
                table: "PartDefinitions");
        }
    }
}
