using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class Production_Step_Workflow_Change : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkInstructions_LineOperators_OperatorId",
                table: "WorkInstructions");

            migrationBuilder.AlterColumn<int>(
                name: "OperatorId",
                table: "WorkInstructions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "ProductionLogStep",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionLogId = table.Column<int>(type: "int", nullable: false),
                    WorkInstructionStepId = table.Column<int>(type: "int", nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionLogStep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionLogStep_ProductionLogs_ProductionLogId",
                        column: x => x.ProductionLogId,
                        principalTable: "ProductionLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionLogStep_Steps_WorkInstructionStepId",
                        column: x => x.WorkInstructionStepId,
                        principalTable: "Steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogStep_ProductionLogId",
                table: "ProductionLogStep",
                column: "ProductionLogId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogStep_WorkInstructionStepId",
                table: "ProductionLogStep",
                column: "WorkInstructionStepId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkInstructions_LineOperators_OperatorId",
                table: "WorkInstructions",
                column: "OperatorId",
                principalTable: "LineOperators",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkInstructions_LineOperators_OperatorId",
                table: "WorkInstructions");

            migrationBuilder.DropTable(
                name: "ProductionLogStep");

            migrationBuilder.AlterColumn<int>(
                name: "OperatorId",
                table: "WorkInstructions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkInstructions_LineOperators_OperatorId",
                table: "WorkInstructions",
                column: "OperatorId",
                principalTable: "LineOperators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
