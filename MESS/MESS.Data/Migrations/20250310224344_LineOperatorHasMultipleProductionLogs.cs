using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class LineOperatorHasMultipleProductionLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LineOperators_ProductionLogs_ProductionLogId",
                table: "LineOperators");

            migrationBuilder.DropIndex(
                name: "IX_LineOperators_ProductionLogId",
                table: "LineOperators");

            migrationBuilder.DropColumn(
                name: "ProductionLogId",
                table: "LineOperators");

            migrationBuilder.AddColumn<int>(
                name: "LineOperatorId",
                table: "ProductionLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_LineOperatorId",
                table: "ProductionLogs",
                column: "LineOperatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogs_LineOperators_LineOperatorId",
                table: "ProductionLogs",
                column: "LineOperatorId",
                principalTable: "LineOperators",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogs_LineOperators_LineOperatorId",
                table: "ProductionLogs");

            migrationBuilder.DropIndex(
                name: "IX_ProductionLogs_LineOperatorId",
                table: "ProductionLogs");

            migrationBuilder.DropColumn(
                name: "LineOperatorId",
                table: "ProductionLogs");

            migrationBuilder.AddColumn<int>(
                name: "ProductionLogId",
                table: "LineOperators",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LineOperators_ProductionLogId",
                table: "LineOperators",
                column: "ProductionLogId",
                unique: true,
                filter: "[ProductionLogId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_LineOperators_ProductionLogs_ProductionLogId",
                table: "LineOperators",
                column: "ProductionLogId",
                principalTable: "ProductionLogs",
                principalColumn: "Id");
        }
    }
}
