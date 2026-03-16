using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class IsActiveWorkInstruction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperatorId",
                table: "WorkInstructions");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "WorkInstructions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "WorkInstructions");

            migrationBuilder.AddColumn<string>(
                name: "OperatorId",
                table: "WorkInstructions",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
