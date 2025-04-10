using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class PrimarySecondaryMediaReplaceContent_Step : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Steps");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryMedia",
                table: "Steps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryMedia",
                table: "Steps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryMedia",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "SecondaryMedia",
                table: "Steps");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Steps",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
