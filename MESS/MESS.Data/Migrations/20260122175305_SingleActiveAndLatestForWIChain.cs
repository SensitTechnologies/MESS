using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class SingleActiveAndLatestForWIChain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add computed ChainId (root of the version chain)
            migrationBuilder.AddColumn<int>(
                name: "ChainId",
                table: "WorkInstructions",
                type: "int",
                nullable: false,
                computedColumnSql: "COALESCE([OriginalId], [Id])",
                stored: true);

            // Only ONE Active WorkInstruction per chain
            migrationBuilder.CreateIndex(
                name: "UX_WorkInstructions_OneActivePerChain",
                table: "WorkInstructions",
                column: "ChainId",
                unique: true,
                filter: "[IsActive] = 1");

            // Only ONE Latest WorkInstruction per chain
            migrationBuilder.CreateIndex(
                name: "UX_WorkInstructions_OneLatestPerChain",
                table: "WorkInstructions",
                column: "ChainId",
                unique: true,
                filter: "[IsLatest] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_WorkInstructions_OneLatestPerChain",
                table: "WorkInstructions");

            migrationBuilder.DropIndex(
                name: "UX_WorkInstructions_OneActivePerChain",
                table: "WorkInstructions");

            migrationBuilder.DropColumn(
                name: "ChainId",
                table: "WorkInstructions");
        }
    }
}
