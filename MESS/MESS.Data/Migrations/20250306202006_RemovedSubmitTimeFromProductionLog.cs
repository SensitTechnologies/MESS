using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovedSubmitTimeFromProductionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubmitTime",
                table: "ProductionLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SubmitTime",
                table: "ProductionLogs",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
