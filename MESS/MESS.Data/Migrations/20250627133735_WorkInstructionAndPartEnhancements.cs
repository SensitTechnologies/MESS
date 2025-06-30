using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class WorkInstructionAndPartEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SerialNumberLogs_Parts_PartId",
                table: "SerialNumberLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SerialNumberLogs",
                table: "SerialNumberLogs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SerialNumberLogs");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "SerialNumberLogs");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "SerialNumberLogs");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "SerialNumberLogs");

            migrationBuilder.DropColumn(
                name: "ProductSerialNumber",
                table: "SerialNumberLogs");

            migrationBuilder.RenameTable(
                name: "SerialNumberLogs",
                newName: "ProductionLogParts");

            migrationBuilder.RenameIndex(
                name: "IX_SerialNumberLogs_PartId",
                table: "ProductionLogParts",
                newName: "IX_ProductionLogParts_PartId");

            migrationBuilder.AddColumn<bool>(
                name: "CollectsProductSerialNumber",
                table: "WorkInstructions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLatest",
                table: "WorkInstructions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OriginalId",
                table: "WorkInstructions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldGenerateQrCode",
                table: "WorkInstructions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProductSerialNumber",
                table: "ProductionLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductionLogParts",
                table: "ProductionLogParts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_WorkInstructions_OriginalId",
                table: "WorkInstructions",
                column: "OriginalId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogParts_Parts_PartId",
                table: "ProductionLogParts",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkInstructions_WorkInstructions_OriginalId",
                table: "WorkInstructions",
                column: "OriginalId",
                principalTable: "WorkInstructions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogParts_Parts_PartId",
                table: "ProductionLogParts");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkInstructions_WorkInstructions_OriginalId",
                table: "WorkInstructions");

            migrationBuilder.DropIndex(
                name: "IX_WorkInstructions_OriginalId",
                table: "WorkInstructions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductionLogParts",
                table: "ProductionLogParts");

            migrationBuilder.DropColumn(
                name: "CollectsProductSerialNumber",
                table: "WorkInstructions");

            migrationBuilder.DropColumn(
                name: "IsLatest",
                table: "WorkInstructions");

            migrationBuilder.DropColumn(
                name: "OriginalId",
                table: "WorkInstructions");

            migrationBuilder.DropColumn(
                name: "ShouldGenerateQrCode",
                table: "WorkInstructions");

            migrationBuilder.DropColumn(
                name: "ProductSerialNumber",
                table: "ProductionLogs");

            migrationBuilder.RenameTable(
                name: "ProductionLogParts",
                newName: "SerialNumberLogs");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionLogParts_PartId",
                table: "SerialNumberLogs",
                newName: "IX_SerialNumberLogs_PartId");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                table: "Products",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastModifiedOn",
                table: "Products",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Parts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                table: "Parts",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Parts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastModifiedOn",
                table: "Parts",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SerialNumberLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                table: "SerialNumberLogs",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "SerialNumberLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastModifiedOn",
                table: "SerialNumberLogs",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "ProductSerialNumber",
                table: "SerialNumberLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SerialNumberLogs",
                table: "SerialNumberLogs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SerialNumberLogs_Parts_PartId",
                table: "SerialNumberLogs",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id");
        }
    }
}
