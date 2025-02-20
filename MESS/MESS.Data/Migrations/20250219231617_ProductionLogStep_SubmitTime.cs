using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProductionLogStep_SubmitTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ProductionLogStep");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "ProductionLogStep");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Steps",
                newName: "SubmitTime");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SubmitTime",
                table: "ProductionLogStep",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubmitTime",
                table: "ProductionLogStep");

            migrationBuilder.RenameColumn(
                name: "SubmitTime",
                table: "Steps",
                newName: "StartTime");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EndTime",
                table: "Steps",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EndTime",
                table: "ProductionLogStep",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartTime",
                table: "ProductionLogStep",
                type: "datetimeoffset",
                nullable: true);
        }
    }
}
