using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE VIEW ProductionView AS
            SELECT 
                dbo.Products.Name AS Product, 
                dbo.WorkInstructions.Title AS WorkInstruction, 
                dbo.Steps.Name AS Step, 
                dbo.ProductionLogStepAttempts.Success, 
                dbo.ProductionLogStepAttempts.Notes, 
                dbo.ProductionLogStepAttempts.SubmitTime, 
                dbo.ProductionLogs.ProductSerialNumber, 
                dbo.AspNetUsers.UserName, 
                dbo.AspNetUsers.FirstName, 
                dbo.AspNetUsers.LastName
            FROM dbo.ProductionLogs
            INNER JOIN dbo.Products ON dbo.ProductionLogs.ProductId = dbo.Products.Id
            INNER JOIN dbo.WorkInstructions ON dbo.ProductionLogs.WorkInstructionId = dbo.WorkInstructions.Id
            INNER JOIN dbo.ProductionLogSteps ON dbo.ProductionLogs.Id = dbo.ProductionLogSteps.ProductionLogId
            INNER JOIN dbo.ProductionLogStepAttempts ON dbo.ProductionLogSteps.Id = dbo.ProductionLogStepAttempts.ProductionLogStepId
            INNER JOIN dbo.Steps ON dbo.ProductionLogSteps.WorkInstructionStepId = dbo.Steps.Id
            INNER JOIN dbo.AspNetUsers ON dbo.ProductionLogs.OperatorId = dbo.AspNetUsers.Id;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            DROP VIEW IF EXISTS ProductionView;
            ");
        }
    }
}
