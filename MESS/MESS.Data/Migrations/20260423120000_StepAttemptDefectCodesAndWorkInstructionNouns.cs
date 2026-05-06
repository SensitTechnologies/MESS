using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations;

/// <summary>
/// Adds nullable defect noun/adjective FKs on step attempts and a many-to-many
/// table linking work instructions to failure nouns available in production.
/// </summary>
public partial class StepAttemptDefectCodesAndWorkInstructionNouns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "WorkInstructionFailureNouns",
            columns: table => new
            {
                FailureNounsId = table.Column<int>(type: "integer", nullable: false),
                WorkInstructionsId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkInstructionFailureNouns", x => new { x.FailureNounsId, x.WorkInstructionsId });
                table.ForeignKey(
                    name: "FK_WorkInstructionFailureNouns_FailureNoun_FailureNounsId",
                    column: x => x.FailureNounsId,
                    principalTable: "FailureNoun",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_WorkInstructionFailureNouns_WorkInstructions_WorkInstructionsId",
                    column: x => x.WorkInstructionsId,
                    principalTable: "WorkInstructions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_WorkInstructionFailureNouns_WorkInstructionsId",
            table: "WorkInstructionFailureNouns",
            column: "WorkInstructionsId");

        migrationBuilder.AddColumn<int>(
            name: "FailureAdjectiveId",
            table: "ProductionLogStepAttempts",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "FailureNounId",
            table: "ProductionLogStepAttempts",
            type: "integer",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_ProductionLogStepAttempts_FailureAdjectiveId",
            table: "ProductionLogStepAttempts",
            column: "FailureAdjectiveId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductionLogStepAttempts_FailureNounId",
            table: "ProductionLogStepAttempts",
            column: "FailureNounId");

        migrationBuilder.AddForeignKey(
            name: "FK_ProductionLogStepAttempts_FailureAdjective_FailureAdjectiveId",
            table: "ProductionLogStepAttempts",
            column: "FailureAdjectiveId",
            principalTable: "FailureAdjective",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);

        migrationBuilder.AddForeignKey(
            name: "FK_ProductionLogStepAttempts_FailureNoun_FailureNounId",
            table: "ProductionLogStepAttempts",
            column: "FailureNounId",
            principalTable: "FailureNoun",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ProductionLogStepAttempts_FailureAdjective_FailureAdjectiveId",
            table: "ProductionLogStepAttempts");

        migrationBuilder.DropForeignKey(
            name: "FK_ProductionLogStepAttempts_FailureNoun_FailureNounId",
            table: "ProductionLogStepAttempts");

        migrationBuilder.DropTable(
            name: "WorkInstructionFailureNouns");

        migrationBuilder.DropIndex(
            name: "IX_ProductionLogStepAttempts_FailureAdjectiveId",
            table: "ProductionLogStepAttempts");

        migrationBuilder.DropIndex(
            name: "IX_ProductionLogStepAttempts_FailureNounId",
            table: "ProductionLogStepAttempts");

        migrationBuilder.DropColumn(
            name: "FailureAdjectiveId",
            table: "ProductionLogStepAttempts");

        migrationBuilder.DropColumn(
            name: "FailureNounId",
            table: "ProductionLogStepAttempts");
    }
}
