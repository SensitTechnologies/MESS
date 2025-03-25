using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations.User
{
    /// <inheritdoc />
    public partial class PendingUserModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductionLogId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "ProductionLogIds",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductionLogIds",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "ProductionLogId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }
    }
}
