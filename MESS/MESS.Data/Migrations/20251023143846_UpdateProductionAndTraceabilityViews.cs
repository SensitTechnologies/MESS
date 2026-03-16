using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductionAndTraceabilityViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.ProductionView', 'V') IS NOT NULL
                    DROP VIEW dbo.ProductionView;
            ");

            migrationBuilder.Sql(@"
                EXEC('
                    CREATE VIEW dbo.ProductionView AS
                    SELECT
                        pd.Name AS Product,
                        wi.Title AS WorkInstruction,
                        s.Name AS Step,
                        sa.Success,
                        sa.Notes,
                        sa.SubmitTime,
                        u.UserName,
                        u.FirstName,
                        u.LastName
                    FROM dbo.ProductionLogs AS pl
                    INNER JOIN dbo.Products AS p ON pl.ProductId = p.Id
                    INNER JOIN dbo.PartDefinitions AS pd ON p.PartDefinitionId = pd.Id
                    INNER JOIN dbo.WorkInstructions AS wi ON pl.WorkInstructionId = wi.Id
                    INNER JOIN dbo.ProductionLogSteps AS pls ON pl.Id = pls.ProductionLogId
                    INNER JOIN dbo.ProductionLogStepAttempts AS sa ON pls.Id = sa.ProductionLogStepId
                    INNER JOIN dbo.Steps AS s ON pls.WorkInstructionStepId = s.Id
                    INNER JOIN dbo.AspNetUsers AS u ON pl.OperatorId = u.Id
                ');
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.TraceabilityView', 'V') IS NOT NULL
                    DROP VIEW dbo.TraceabilityView;
            ");

            migrationBuilder.Sql(@"
                EXEC('
                    CREATE VIEW dbo.TraceabilityView AS
                    SELECT
                        pDef.Name AS Product,
                        pl.CreatedOn AS SubmitTime,
                        partDef.Name AS PartName,
                        partDef.Number AS PartNumber,
                        sp.SerialNumber AS PartSerialNumber,
                        parentDef.Name AS ParentPartName,
                        parentSp.SerialNumber AS ParentSerialNumber,
                        CASE plp.OperationType
                            WHEN 0 THEN ''Installed''
                            WHEN 1 THEN ''Produced''
                            WHEN 2 THEN ''Removed''
                        END AS OperationType,
                        u.FirstName,
                        u.LastName,
                        u.UserName
                    FROM dbo.ProductionLogParts AS plp
                    INNER JOIN dbo.ProductionLogs AS pl ON plp.ProductionLogId = pl.Id
                    INNER JOIN dbo.Products AS p ON pl.ProductId = p.Id
                    INNER JOIN dbo.PartDefinitions AS pDef ON p.PartDefinitionId = pDef.Id
                    LEFT JOIN dbo.SerializableParts AS sp ON plp.SerializablePartId = sp.Id
                    LEFT JOIN dbo.PartDefinitions AS partDef ON sp.PartDefinitionId = partDef.Id
                    LEFT JOIN dbo.SerializableParts AS parentSp ON plp.ParentPartId = parentSp.Id
                    LEFT JOIN dbo.PartDefinitions AS parentDef ON parentSp.PartDefinitionId = parentDef.Id
                    LEFT JOIN dbo.AspNetUsers AS u ON pl.OperatorId = u.Id
                ');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore old ProductionView
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.ProductionView', 'V') IS NOT NULL
                    DROP VIEW dbo.ProductionView;
            ");

            migrationBuilder.Sql(@"
                EXEC('
                    CREATE VIEW dbo.ProductionView AS
                    SELECT 
                        p.Name AS Product, 
                        wi.Title AS WorkInstruction, 
                        s.Name AS Step, 
                        sa.Success, 
                        sa.Notes, 
                        sa.SubmitTime, 
                        pl.ProductSerialNumber, 
                        u.UserName, 
                        u.FirstName, 
                        u.LastName
                    FROM dbo.ProductionLogs AS pl
                    INNER JOIN dbo.Products AS p ON pl.ProductId = p.Id
                    INNER JOIN dbo.WorkInstructions AS wi ON pl.WorkInstructionId = wi.Id
                    INNER JOIN dbo.ProductionLogSteps AS pls ON pl.Id = pls.ProductionLogId
                    INNER JOIN dbo.ProductionLogStepAttempts AS sa ON pls.Id = sa.ProductionLogStepId
                    INNER JOIN dbo.Steps AS s ON pls.WorkInstructionStepId = s.Id
                    INNER JOIN dbo.AspNetUsers AS u ON pl.OperatorId = u.Id
                ');
            ");

            // Restore old TraceabilityView
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.TraceabilityView', 'V') IS NOT NULL
                    DROP VIEW dbo.TraceabilityView;
            ");

            migrationBuilder.Sql(@"
                EXEC('
                    CREATE VIEW dbo.TraceabilityView AS
                    SELECT 
                        p.Name AS Product, 
                        plp.SubmitTimeQc AS SubmitTime, 
                        pa.PartName, 
                        pa.PartNumber, 
                        plp.PartSerialNumber, 
                        u.FirstName, 
                        u.LastName, 
                        u.UserName, 
                        pl.ProductSerialNumber
                    FROM dbo.ProductionLogs AS pl
                    LEFT JOIN dbo.ProductionLogParts AS plp ON plp.ProductionLogId = pl.Id
                    INNER JOIN dbo.Products AS p ON pl.ProductId = p.Id
                    LEFT JOIN dbo.Parts AS pa ON plp.PartId = pa.Id
                    LEFT JOIN dbo.AspNetUsers AS u ON pl.OperatorId = u.Id
                    WHERE (plp.PartSerialNumber IS NOT NULL) 
                       OR (pl.ProductSerialNumber IS NOT NULL)
                ');
            ");
        }
    }
}
