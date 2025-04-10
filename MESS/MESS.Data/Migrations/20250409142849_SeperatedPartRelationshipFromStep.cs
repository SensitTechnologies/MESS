using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeperatedPartRelationshipFromStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogStep_Steps_WorkInstructionStepId",
                table: "ProductionLogStep");

            migrationBuilder.DropForeignKey(
                name: "FK_Parts_Steps_StepId",
                table: "Parts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Steps",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Steps");

            migrationBuilder.DropForeignKey(
                name: "FK_Steps_WorkInstructions_WorkInstructionId",
                table: "Steps");

            migrationBuilder.DropIndex(
                name: "IX_Steps_WorkInstructionId",
                table: "Steps");

            migrationBuilder.DropIndex(
                name: "IX_Parts_StepId",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "WorkInstructionId",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "StepId",
                table: "Parts");

            migrationBuilder.AddColumn<bool>(
                name: "PartsRequired",
                table: "WorkInstructions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "WorkInstructionNodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Position = table.Column<int>(type: "int", nullable: false),
                    NodeType = table.Column<int>(type: "int", nullable: false),
                    WorkInstructionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkInstructionNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkInstructionNodes_WorkInstructions_WorkInstructionId",
                        column: x => x.WorkInstructionId,
                        principalTable: "WorkInstructions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PartNodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartNodes_WorkInstructionNodes_Id",
                        column: x => x.Id,
                        principalTable: "WorkInstructionNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartNodeParts",
                columns: table => new
                {
                    PartNodeId = table.Column<int>(type: "int", nullable: false),
                    PartsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartNodeParts", x => new { x.PartNodeId, x.PartsId });
                    table.ForeignKey(
                        name: "FK_PartNodeParts_PartNodes_PartNodeId",
                        column: x => x.PartNodeId,
                        principalTable: "PartNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartNodeParts_Parts_PartsId",
                        column: x => x.PartsId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Steps",
                type: "int",
                nullable: false);
            
            migrationBuilder.AddPrimaryKey(
                name: "PK_Steps",
                table: "Steps",
                column: "Id");
            
            migrationBuilder.AddForeignKey(
                name: "FK_Steps_WorkInstructionNodes_Id",
                table: "Steps",
                column: "Id",
                principalTable: "WorkInstructionNodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_PartNodeParts_PartsId",
                table: "PartNodeParts",
                column: "PartsId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkInstructionNodes_WorkInstructionId",
                table: "WorkInstructionNodes",
                column: "WorkInstructionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                    name: "Id",
                    table: "Steps",
                    type: "int",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");
            
            migrationBuilder.DropForeignKey(
                name: "FK_Steps_WorkInstructionNodes_Id",
                table: "Steps");

            migrationBuilder.DropTable(
                name: "PartNodeParts");

            migrationBuilder.DropTable(
                name: "PartNodes");

            migrationBuilder.DropTable(
                name: "WorkInstructionNodes");

            migrationBuilder.DropColumn(
                name: "PartsRequired",
                table: "WorkInstructions");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Steps",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Steps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                table: "Steps",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Steps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastModifiedOn",
                table: "Steps",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "WorkInstructionId",
                table: "Steps",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Parts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "StepId",
                table: "Parts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Steps_WorkInstructionId",
                table: "Steps",
                column: "WorkInstructionId");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_StepId",
                table: "Parts",
                column: "StepId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_Steps_StepId",
                table: "Parts",
                column: "StepId",
                principalTable: "Steps",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_WorkInstructions_WorkInstructionId",
                table: "Steps",
                column: "WorkInstructionId",
                principalTable: "WorkInstructions",
                principalColumn: "Id");
        }
    }
}
