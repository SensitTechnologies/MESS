using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CellCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cells", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Problems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProblemCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Problems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootCauses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SerialNumberLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductionLogId = table.Column<int>(type: "int", nullable: false),
                    SubmitTimeQc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialNumberLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkStations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkStations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProblemProduct",
                columns: table => new
                {
                    ProblemsId = table.Column<int>(type: "int", nullable: false),
                    ProductsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProblemProduct", x => new { x.ProblemsId, x.ProductsId });
                    table.ForeignKey(
                        name: "FK_ProblemProduct_Problems_ProblemsId",
                        column: x => x.ProblemsId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProblemProduct_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "Documentations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExternalLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkInstructionId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LineOperators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ProductionLogId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineOperators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkInstructions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperatorId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkInstructions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkInstructions_LineOperators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "LineOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkInstructions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmitTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    WorkStationId = table.Column<int>(type: "int", nullable: true),
                    WorkInstructionId = table.Column<int>(type: "int", nullable: true),
                    ProductStatusId = table.Column<int>(type: "int", nullable: true),
                    ProblemId = table.Column<int>(type: "int", nullable: true),
                    RootCauseId = table.Column<int>(type: "int", nullable: true),
                    CellId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionLogs_Cells_CellId",
                        column: x => x.CellId,
                        principalTable: "Cells",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionLogs_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionLogs_ProductStatus_ProductStatusId",
                        column: x => x.ProductStatusId,
                        principalTable: "ProductStatus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionLogs_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionLogs_RootCauses_RootCauseId",
                        column: x => x.RootCauseId,
                        principalTable: "RootCauses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionLogs_WorkInstructions_WorkInstructionId",
                        column: x => x.WorkInstructionId,
                        principalTable: "WorkInstructions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionLogs_WorkStations_WorkStationId",
                        column: x => x.WorkStationId,
                        principalTable: "WorkStations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Steps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    WorkInstructionId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Steps_WorkInstructions_WorkInstructionId",
                        column: x => x.WorkInstructionId,
                        principalTable: "WorkInstructions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Parts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LogId = table.Column<int>(type: "int", nullable: false),
                    StepId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parts_SerialNumberLogs_LogId",
                        column: x => x.LogId,
                        principalTable: "SerialNumberLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Parts_Steps_StepId",
                        column: x => x.StepId,
                        principalTable: "Steps",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documentations_WorkInstructionId",
                table: "Documentations",
                column: "WorkInstructionId");

            migrationBuilder.CreateIndex(
                name: "IX_LineOperators_ProductionLogId",
                table: "LineOperators",
                column: "ProductionLogId",
                unique: true,
                filter: "[ProductionLogId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_LogId",
                table: "Parts",
                column: "LogId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parts_StepId",
                table: "Parts",
                column: "StepId");

            migrationBuilder.CreateIndex(
                name: "IX_ProblemProduct_ProductsId",
                table: "ProblemProduct",
                column: "ProductsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_CellId",
                table: "ProductionLogs",
                column: "CellId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_ProblemId",
                table: "ProductionLogs",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_ProductId",
                table: "ProductionLogs",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_ProductStatusId",
                table: "ProductionLogs",
                column: "ProductStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_RootCauseId",
                table: "ProductionLogs",
                column: "RootCauseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_WorkInstructionId",
                table: "ProductionLogs",
                column: "WorkInstructionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_WorkStationId",
                table: "ProductionLogs",
                column: "WorkStationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductWorkStation_WorkStationsId",
                table: "ProductWorkStation",
                column: "WorkStationsId");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_WorkInstructionId",
                table: "Steps",
                column: "WorkInstructionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkInstructions_OperatorId",
                table: "WorkInstructions",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkInstructions_ProductId",
                table: "WorkInstructions",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documentations_WorkInstructions_WorkInstructionId",
                table: "Documentations",
                column: "WorkInstructionId",
                principalTable: "WorkInstructions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LineOperators_ProductionLogs_ProductionLogId",
                table: "LineOperators",
                column: "ProductionLogId",
                principalTable: "ProductionLogs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogs_WorkInstructions_WorkInstructionId",
                table: "ProductionLogs");

            migrationBuilder.DropTable(
                name: "Documentations");

            migrationBuilder.DropTable(
                name: "Parts");

            migrationBuilder.DropTable(
                name: "ProblemProduct");

            migrationBuilder.DropTable(
                name: "ProductWorkStation");

            migrationBuilder.DropTable(
                name: "SerialNumberLogs");

            migrationBuilder.DropTable(
                name: "Steps");

            migrationBuilder.DropTable(
                name: "WorkInstructions");

            migrationBuilder.DropTable(
                name: "LineOperators");

            migrationBuilder.DropTable(
                name: "ProductionLogs");

            migrationBuilder.DropTable(
                name: "Cells");

            migrationBuilder.DropTable(
                name: "Problems");

            migrationBuilder.DropTable(
                name: "ProductStatus");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "RootCauses");

            migrationBuilder.DropTable(
                name: "WorkStations");
        }
    }
}
