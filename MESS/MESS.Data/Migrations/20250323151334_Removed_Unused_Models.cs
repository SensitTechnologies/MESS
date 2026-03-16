using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class Removed_Unused_Models : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogs_Cells_CellId",
                table: "ProductionLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogs_Problems_ProblemId",
                table: "ProductionLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogs_ProductStatus_ProductStatusId",
                table: "ProductionLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogs_RootCauses_RootCauseId",
                table: "ProductionLogs");

            migrationBuilder.DropTable(
                name: "Cells");

            migrationBuilder.DropTable(
                name: "Documentations");

            migrationBuilder.DropTable(
                name: "ProductStatus");

            migrationBuilder.DropTable(
                name: "RootCauses");

            migrationBuilder.DropIndex(
                name: "IX_ProductionLogs_CellId",
                table: "ProductionLogs");

            migrationBuilder.DropIndex(
                name: "IX_ProductionLogs_ProblemId",
                table: "ProductionLogs");

            migrationBuilder.DropIndex(
                name: "IX_ProductionLogs_ProductStatusId",
                table: "ProductionLogs");

            migrationBuilder.DropIndex(
                name: "IX_ProductionLogs_RootCauseId",
                table: "ProductionLogs");

            migrationBuilder.DropColumn(
                name: "CellId",
                table: "ProductionLogs");

            migrationBuilder.DropColumn(
                name: "ProblemId",
                table: "ProductionLogs");

            migrationBuilder.DropColumn(
                name: "ProductStatusId",
                table: "ProductionLogs");

            migrationBuilder.DropColumn(
                name: "RootCauseId",
                table: "ProductionLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CellId",
                table: "ProductionLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProblemId",
                table: "ProductionLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductStatusId",
                table: "ProductionLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RootCauseId",
                table: "ProductionLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CellCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cells", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documentations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExternalLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkInstructionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documentations_WorkInstructions_WorkInstructionId",
                        column: x => x.WorkInstructionId,
                        principalTable: "WorkInstructions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    StatusCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RootCauses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CauseCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootCauses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_CellId",
                table: "ProductionLogs",
                column: "CellId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_ProblemId",
                table: "ProductionLogs",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_ProductStatusId",
                table: "ProductionLogs",
                column: "ProductStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_RootCauseId",
                table: "ProductionLogs",
                column: "RootCauseId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentations_WorkInstructionId",
                table: "Documentations",
                column: "WorkInstructionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogs_Cells_CellId",
                table: "ProductionLogs",
                column: "CellId",
                principalTable: "Cells",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogs_Problems_ProblemId",
                table: "ProductionLogs",
                column: "ProblemId",
                principalTable: "Problems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogs_ProductStatus_ProductStatusId",
                table: "ProductionLogs",
                column: "ProductStatusId",
                principalTable: "ProductStatus",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogs_RootCauses_RootCauseId",
                table: "ProductionLogs",
                column: "RootCauseId",
                principalTable: "RootCauses",
                principalColumn: "Id");
        }
    }
}
