using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameStepColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Rename Body to DetailedBody
            migrationBuilder.RenameColumn(
                name: "Body",
                table: "Steps",
                newName: "DetailedBody");

            // 2. Add new required Body column (non-nullable string)
            // Add with a temporary default value to avoid issues on non-nullable
            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "Steps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // 3. Copy values from Name to new Body column
            migrationBuilder.Sql(
                @"
            UPDATE Steps
            SET Body = Name
            WHERE Body = ''
            ");

            // 4. (Optional) If Name was nullable and is now required, handle accordingly
            // Assuming no change to Name column nullability here.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse steps:
            migrationBuilder.DropColumn(
                name: "Body",
                table: "Steps");

            migrationBuilder.RenameColumn(
                name: "DetailedBody",
                table: "Steps",
                newName: "Body");
        }
    }
}
