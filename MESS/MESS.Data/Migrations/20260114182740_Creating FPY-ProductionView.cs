using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreatingFPYProductionView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE VIEW dbo.FPYAttemptsView AS
                WITH FirstAttempts AS (
                    SELECT
                        pla.Id,
                        pla.ProductionLogStepId,
                        pla.Success,
                        pla.Notes,
                        pla.SubmitTime,
                        ROW_NUMBER() OVER (
                            PARTITION BY pla.ProductionLogStepId
                            ORDER BY pla.SubmitTime
                        ) AS rn
                    FROM dbo.ProductionLogStepAttempts pla
                )
                SELECT 
                    pl.Id AS ProductionLogId,
                    pd.Name AS Product,
                    wi.Title AS WorkInstruction,
                    wi.Version AS WorkInstructionVersion,
                    s.Id AS StepId,
                    s.Name AS Step,
                    fa.Success,
                    fa.Notes,
                    fa.SubmitTime,
                    pl.FromBatchOf,
                    u.UserName,
                    u.FirstName,
                    u.LastName
                FROM dbo.ProductionLogs pl
                INNER JOIN dbo.Products p
                    ON pl.ProductId = p.Id
                INNER JOIN dbo.PartDefinitions pd
                    ON p.PartDefinitionId = pd.Id
                INNER JOIN dbo.WorkInstructions wi
                    ON pl.WorkInstructionId = wi.Id
                INNER JOIN dbo.ProductionLogSteps pls
                    ON pl.Id = pls.ProductionLogId
                INNER JOIN FirstAttempts fa
                    ON pls.Id = fa.ProductionLogStepId
                    AND fa.rn = 1
                INNER JOIN dbo.Steps s
                    ON pls.WorkInstructionStepId = s.Id
                LEFT JOIN dbo.AspNetUsers u
                    ON pl.OperatorId = u.Id;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.FPYAttemptsView;");
        }
    }
}
