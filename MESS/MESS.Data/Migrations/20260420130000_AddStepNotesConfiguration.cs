using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations;

/// <summary>
/// Adds per-step operator notes configuration for production (My Production).
/// </summary>
public partial class AddStepNotesConfiguration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "NotesConfiguration",
            table: "Steps",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "NotesConfiguration",
            table: "Steps");
    }
}
