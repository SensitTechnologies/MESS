using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTraceabilityViewForNoParentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                        LEFT JOIN dbo.AspNetUsers AS u ON pl.OperatorId = u.Id
                ');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
