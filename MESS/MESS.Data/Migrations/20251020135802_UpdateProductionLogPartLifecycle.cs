using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductionLogPartLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- STEP 1: Drop conflicting constraints early ---
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogParts_PartDefinitions_PartId",
                table: "ProductionLogParts");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogParts_SerializableParts_SerializablePartId",
                table: "ProductionLogParts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductionLogParts",
                table: "ProductionLogParts");

            // --- STEP 2: Data preservation before dropping old columns ---
            migrationBuilder.AddColumn<int>(
                name: "OperationType",
                table: "ProductionLogParts",
                type: "int",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.AddColumn<int>(
                name: "ParentPartId",
                table: "ProductionLogParts",
                type: "int",
                nullable: true); // Start nullable, will point to SerializableParts later
            
            migrationBuilder.Sql(@"
                INSERT INTO SerializableParts (PartId, SerialNumber)
                SELECT DISTINCT p.PartId, p.PartSerialNumber
                FROM ProductionLogParts p
                WHERE p.PartSerialNumber IS NOT NULL
                  AND NOT EXISTS (
                    SELECT 1 FROM SerializableParts s
                    WHERE s.SerialNumber = p.PartSerialNumber
                );
            ");

            migrationBuilder.Sql(@"
                UPDATE p
                SET p.SerializablePartId = s.Id
                FROM ProductionLogParts p
                JOIN SerializableParts s ON s.SerialNumber = p.PartSerialNumber
                WHERE p.PartSerialNumber IS NOT NULL;
            ");
            
            // Delete invalid rows where old PartSerialNumber is set to null
            migrationBuilder.Sql(@"
                DELETE FROM ProductionLogParts
                WHERE SerializablePartId IS NULL;
            ");

            // --- STEP 3: Remove duplicates before new PK ---
            migrationBuilder.Sql(@"
                WITH Duplicates AS (
                    SELECT ProductionLogId, SerializablePartId, OperationType,
                           ROW_NUMBER() OVER (PARTITION BY ProductionLogId, SerializablePartId, OperationType ORDER BY (SELECT NULL)) AS rn
                    FROM ProductionLogParts
                )
                DELETE FROM Duplicates WHERE rn > 1;
            ");

            // --- STEP 4: Modify schema ---
            migrationBuilder.DropColumn(
                name: "Id",
                table: "ProductionLogParts");

            migrationBuilder.DropColumn(
                name: "PartSerialNumber",
                table: "ProductionLogParts");

            migrationBuilder.DropColumn(
                name: "SubmitTimeQc",
                table: "ProductionLogParts");
            
            migrationBuilder.DropIndex(
                name: "IX_ProductionLogParts_PartId",
                table: "ProductionLogParts");
            
            migrationBuilder.DropColumn(
                name: "PartId",
                table: "ProductionLogParts");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogParts_ParentPartId",
                table: "ProductionLogParts",
                column: "ParentPartId");

            migrationBuilder.AlterColumn<int>(
                name: "SerializablePartId",
                table: "ProductionLogParts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // --- STEP 5: Add new PK safely ---
            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductionLogParts",
                table: "ProductionLogParts",
                columns: new[] { "ProductionLogId", "SerializablePartId", "OperationType" });

            // --- STEP 6: Recreate foreign keys ---
            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogParts_ProductionLogs_ProductionLogId",
                table: "ProductionLogParts",
                column: "ProductionLogId",
                principalTable: "ProductionLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogParts_SerializableParts_ParentPartId",
                table: "ProductionLogParts",
                column: "ParentPartId",
                principalTable: "SerializableParts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogParts_SerializableParts_SerializablePartId",
                table: "ProductionLogParts",
                column: "SerializablePartId",
                principalTable: "SerializableParts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
        
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE p
                SET p.PartSerialNumber = s.SerialNumber
                FROM ProductionLogParts p
                JOIN SerializableParts s ON p.SerializablePartId = s.Id;
            ");

            // Drop foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogParts_ProductionLogs_ProductionLogId",
                table: "ProductionLogParts");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogParts_SerializableParts_ParentPartId",
                table: "ProductionLogParts");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogParts_SerializableParts_SerializablePartId",
                table: "ProductionLogParts");

            // Drop primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductionLogParts",
                table: "ProductionLogParts");

            // Drop OperationType and ParentPartId
            migrationBuilder.DropColumn(
                name: "OperationType",
                table: "ProductionLogParts");

            migrationBuilder.DropColumn(
                name: "ParentPartId",
                table: "ProductionLogParts");

            // Make SerializablePartId nullable again
            migrationBuilder.AlterColumn<int>(
                name: "SerializablePartId",
                table: "ProductionLogParts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // Restore Id, PartSerialNumber, SubmitTimeQc
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ProductionLogParts",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "PartSerialNumber",
                table: "ProductionLogParts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SubmitTimeQc",
                table: "ProductionLogParts",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "SYSDATETIMEOFFSET()");

            // Restore primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductionLogParts",
                table: "ProductionLogParts",
                column: "Id");

            // Restore foreign keys
            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogParts_PartDefinitions_PartId",
                table: "ProductionLogParts",
                column: "PartId",
                principalTable: "PartDefinitions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogParts_SerializableParts_SerializablePartId",
                table: "ProductionLogParts",
                column: "SerializablePartId",
                principalTable: "SerializableParts",
                principalColumn: "Id");
        }
    }
}
