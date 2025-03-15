using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovedUnusedSerialNumberLogFieldOnPart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parts_SerialNumberLogs_LogId",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_LogId",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "LogId",
                table: "Parts");

            migrationBuilder.AddColumn<int>(
                name: "PartId",
                table: "SerialNumberLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumberLogs_PartId",
                table: "SerialNumberLogs",
                column: "PartId");

            migrationBuilder.AddForeignKey(
                name: "FK_SerialNumberLogs_Parts_PartId",
                table: "SerialNumberLogs",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SerialNumberLogs_Parts_PartId",
                table: "SerialNumberLogs");

            migrationBuilder.DropIndex(
                name: "IX_SerialNumberLogs_PartId",
                table: "SerialNumberLogs");

            migrationBuilder.DropColumn(
                name: "PartId",
                table: "SerialNumberLogs");

            migrationBuilder.AddColumn<int>(
                name: "LogId",
                table: "Parts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Parts_LogId",
                table: "Parts",
                column: "LogId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_SerialNumberLogs_LogId",
                table: "Parts",
                column: "LogId",
                principalTable: "SerialNumberLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
