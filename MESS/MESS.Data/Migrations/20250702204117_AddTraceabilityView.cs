using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTraceabilityView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE VIEW TraceabilityView AS
            SELECT 
                dbo.Products.Name AS Product, 
                dbo.ProductionLogParts.SubmitTimeQc AS SubmitTime, 
                dbo.Parts.PartName, 
                dbo.Parts.PartNumber, 
                dbo.ProductionLogParts.PartSerialNumber, 
                dbo.AspNetUsers.FirstName, 
                dbo.AspNetUsers.LastName, 
                dbo.AspNetUsers.UserName, 
                dbo.ProductionLogs.ProductSerialNumber
            FROM dbo.ProductionLogs
            LEFT OUTER JOIN dbo.ProductionLogParts ON dbo.ProductionLogParts.ProductionLogId = dbo.ProductionLogs.Id
            INNER JOIN dbo.Products ON dbo.ProductionLogs.ProductId = dbo.Products.Id
            LEFT OUTER JOIN dbo.Parts ON dbo.ProductionLogParts.PartId = dbo.Parts.Id
            LEFT OUTER JOIN dbo.AspNetUsers ON dbo.ProductionLogs.OperatorId = dbo.AspNetUsers.Id
            WHERE (dbo.ProductionLogParts.PartSerialNumber IS NOT NULL) 
               OR (dbo.ProductionLogs.ProductSerialNumber IS NOT NULL);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            DROP VIEW IF EXISTS TraceabilityView;
            ");
        }
    }
}
