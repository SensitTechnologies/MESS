using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class Removed_WorkStation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogs_WorkStations_WorkStationId",
                table: "ProductionLogs");

            migrationBuilder.DropTable(
                name: "ProductWorkStation");

            migrationBuilder.DropTable(
                name: "WorkInstructionWorkStation");

            migrationBuilder.DropTable(
                name: "WorkStations");

            migrationBuilder.DropIndex(
                name: "IX_ProductionLogs_WorkStationId",
                table: "ProductionLogs");

            migrationBuilder.DropColumn(
                name: "WorkStationId",
                table: "ProductionLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkStationId",
                table: "ProductionLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkStations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkStations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductWorkStation",
                columns: table => new
                {
                    ProductsId = table.Column<int>(type: "int", nullable: false),
                    WorkStationsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductWorkStation", x => new { x.ProductsId, x.WorkStationsId });
                    table.ForeignKey(
                        name: "FK_ProductWorkStation_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductWorkStation_WorkStations_WorkStationsId",
                        column: x => x.WorkStationsId,
                        principalTable: "WorkStations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkInstructionWorkStation",
                columns: table => new
                {
                    WorkInstructionsId = table.Column<int>(type: "int", nullable: false),
                    WorkStationsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkInstructionWorkStation", x => new { x.WorkInstructionsId, x.WorkStationsId });
                    table.ForeignKey(
                        name: "FK_WorkInstructionWorkStation_WorkInstructions_WorkInstructionsId",
                        column: x => x.WorkInstructionsId,
                        principalTable: "WorkInstructions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkInstructionWorkStation_WorkStations_WorkStationsId",
                        column: x => x.WorkStationsId,
                        principalTable: "WorkStations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_WorkStationId",
                table: "ProductionLogs",
                column: "WorkStationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductWorkStation_WorkStationsId",
                table: "ProductWorkStation",
                column: "WorkStationsId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkInstructionWorkStation_WorkStationsId",
                table: "WorkInstructionWorkStation",
                column: "WorkStationsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogs_WorkStations_WorkStationId",
                table: "ProductionLogs",
                column: "WorkStationId",
                principalTable: "WorkStations",
                principalColumn: "Id");
        }
    }
}
