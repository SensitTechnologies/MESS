using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSerializablePartParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FK that depends on ParentId
            migrationBuilder.DropForeignKey(
                name: "FK_SerializableParts_SerializableParts_ParentId",
                table: "SerializableParts");

            // Drop index on ParentId
            migrationBuilder.DropIndex(
                name: "IX_SerializableParts_ParentId",
                table: "SerializableParts");

            // Drop the ParentId column
            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "SerializableParts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Re-add the ParentId column
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "SerializableParts",
                type: "int",
                nullable: true);

            // Recreate the index
            migrationBuilder.CreateIndex(
                name: "IX_SerializableParts_ParentId",
                table: "SerializableParts",
                column: "ParentId");

            // Recreate the FK
            migrationBuilder.AddForeignKey(
                name: "FK_SerializableParts_SerializableParts_ParentId",
                table: "SerializableParts",
                column: "ParentId",
                principalTable: "SerializableParts",
                principalColumn: "Id");
        }
    }
}
