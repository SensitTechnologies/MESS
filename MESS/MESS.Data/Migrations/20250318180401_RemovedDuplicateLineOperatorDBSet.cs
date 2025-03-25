using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovedDuplicateLineOperatorDBSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogs_LineOperators_LineOperatorId",
                table: "ProductionLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkInstructions_LineOperators_OperatorId",
                table: "WorkInstructions");

            migrationBuilder.DropTable(
                name: "LineOperators");

            migrationBuilder.DropIndex(
                name: "IX_WorkInstructions_OperatorId",
                table: "WorkInstructions");

            migrationBuilder.DropIndex(
                name: "IX_ProductionLogs_LineOperatorId",
                table: "ProductionLogs");

            migrationBuilder.DropColumn(
                name: "LineOperatorId",
                table: "ProductionLogs");

            migrationBuilder.AlterColumn<string>(
                name: "OperatorId",
                table: "WorkInstructions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatorId",
                table: "ProductionLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperatorId",
                table: "ProductionLogs");

            migrationBuilder.AlterColumn<int>(
                name: "OperatorId",
                table: "WorkInstructions",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LineOperatorId",
                table: "ProductionLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LineOperators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineOperators", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkInstructions_OperatorId",
                table: "WorkInstructions",
                column: "OperatorId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_WorkInstructions_LineOperators_OperatorId",
                table: "WorkInstructions",
                column: "OperatorId",
                principalTable: "LineOperators",
                principalColumn: "Id");
        }
    }
}
