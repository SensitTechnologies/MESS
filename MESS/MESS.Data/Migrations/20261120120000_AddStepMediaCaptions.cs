using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations;

/// <summary>
/// Converts Steps.PrimaryMedia / Steps.SecondaryMedia from text[] (bare image paths) to jsonb arrays of
/// { "Path", "Caption" } so each photo can carry an optional caption. Existing paths are backfilled with a
/// null caption. Down() restores text[] from the paths; captions are lost on rollback.
/// </summary>
public partial class AddStepMediaCaptions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PrimaryMediaJson", table: "Steps", type: "jsonb", nullable: false, defaultValue: "[]");
        migrationBuilder.AddColumn<string>(
            name: "SecondaryMediaJson", table: "Steps", type: "jsonb", nullable: false, defaultValue: "[]");

        // Postgres cannot cast text[] to jsonb directly; COALESCE keeps rows with no images at '[]', not NULL.
        migrationBuilder.Sql(@"
            UPDATE ""Steps""
            SET ""PrimaryMediaJson"" = COALESCE(
                    (SELECT jsonb_agg(jsonb_build_object('Path', p, 'Caption', NULL) ORDER BY ord)
                     FROM unnest(""PrimaryMedia"") WITH ORDINALITY AS t(p, ord)),
                    '[]'::jsonb),
                ""SecondaryMediaJson"" = COALESCE(
                    (SELECT jsonb_agg(jsonb_build_object('Path', p, 'Caption', NULL) ORDER BY ord)
                     FROM unnest(""SecondaryMedia"") WITH ORDINALITY AS t(p, ord)),
                    '[]'::jsonb);
        ");

        migrationBuilder.DropColumn(name: "PrimaryMedia", table: "Steps");
        migrationBuilder.DropColumn(name: "SecondaryMedia", table: "Steps");
        migrationBuilder.RenameColumn(name: "PrimaryMediaJson", table: "Steps", newName: "PrimaryMedia");
        migrationBuilder.RenameColumn(name: "SecondaryMediaJson", table: "Steps", newName: "SecondaryMedia");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string[]>(
            name: "PrimaryMediaArray", table: "Steps", type: "text[]", nullable: false, defaultValue: new string[0]);
        migrationBuilder.AddColumn<string[]>(
            name: "SecondaryMediaArray", table: "Steps", type: "text[]", nullable: false, defaultValue: new string[0]);

        migrationBuilder.Sql(@"
            UPDATE ""Steps""
            SET ""PrimaryMediaArray"" = COALESCE(
                    (SELECT array_agg(elem->>'Path' ORDER BY ord)
                     FROM jsonb_array_elements(""PrimaryMedia"") WITH ORDINALITY AS t(elem, ord)),
                    '{}'),
                ""SecondaryMediaArray"" = COALESCE(
                    (SELECT array_agg(elem->>'Path' ORDER BY ord)
                     FROM jsonb_array_elements(""SecondaryMedia"") WITH ORDINALITY AS t(elem, ord)),
                    '{}');
        ");

        migrationBuilder.DropColumn(name: "PrimaryMedia", table: "Steps");
        migrationBuilder.DropColumn(name: "SecondaryMedia", table: "Steps");
        migrationBuilder.RenameColumn(name: "PrimaryMediaArray", table: "Steps", newName: "PrimaryMedia");
        migrationBuilder.RenameColumn(name: "SecondaryMediaArray", table: "Steps", newName: "SecondaryMedia");
    }
}
