using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConvertToStepAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogStep_ProductionLogs_ProductionLogId",
                table: "ProductionLogStep");

            // migrationBuilder.DropForeignKey(
                // name: "FK_ProductionLogStep_Steps_WorkInstructionStepId",
                // table: "ProductionLogStep");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductionLogStep",
                table: "ProductionLogStep");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ProductionLogStep");

            migrationBuilder.DropColumn(
                name: "SubmitTime",
                table: "ProductionLogStep");

            migrationBuilder.DropColumn(
                name: "Success",
                table: "ProductionLogStep");

            migrationBuilder.RenameTable(
                name: "ProductionLogStep",
                newName: "ProductionLogSteps");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionLogStep_WorkInstructionStepId",
                table: "ProductionLogSteps",
                newName: "IX_ProductionLogSteps_WorkInstructionStepId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionLogStep_ProductionLogId",
                table: "ProductionLogSteps",
                newName: "IX_ProductionLogSteps_ProductionLogId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductionLogSteps",
                table: "ProductionLogSteps",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ProductionLogStepAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionLogStepId = table.Column<int>(type: "int", nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmitTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionLogStepAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionLogStepAttempts_ProductionLogSteps_ProductionLogStepId",
                        column: x => x.ProductionLogStepId,
                        principalTable: "ProductionLogSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogStepAttempts_ProductionLogStepId",
                table: "ProductionLogStepAttempts",
                column: "ProductionLogStepId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogSteps_ProductionLogs_ProductionLogId",
                table: "ProductionLogSteps",
                column: "ProductionLogId",
                principalTable: "ProductionLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogSteps_Steps_WorkInstructionStepId",
                table: "ProductionLogSteps",
                column: "WorkInstructionStepId",
                principalTable: "Steps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogSteps_ProductionLogs_ProductionLogId",
                table: "ProductionLogSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogSteps_Steps_WorkInstructionStepId",
                table: "ProductionLogSteps");

            migrationBuilder.DropTable(
                name: "ProductionLogStepAttempts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductionLogSteps",
                table: "ProductionLogSteps");

            migrationBuilder.RenameTable(
                name: "ProductionLogSteps",
                newName: "ProductionLogStep");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionLogSteps_WorkInstructionStepId",
                table: "ProductionLogStep",
                newName: "IX_ProductionLogStep_WorkInstructionStepId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionLogSteps_ProductionLogId",
                table: "ProductionLogStep",
                newName: "IX_ProductionLogStep_ProductionLogId");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ProductionLogStep",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SubmitTime",
                table: "ProductionLogStep",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "Success",
                table: "ProductionLogStep",
                type: "bit",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductionLogStep",
                table: "ProductionLogStep",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogStep_ProductionLogs_ProductionLogId",
                table: "ProductionLogStep",
                column: "ProductionLogId",
                principalTable: "ProductionLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogStep_Steps_WorkInstructionStepId",
                table: "ProductionLogStep",
                column: "WorkInstructionStepId",
                principalTable: "Steps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
