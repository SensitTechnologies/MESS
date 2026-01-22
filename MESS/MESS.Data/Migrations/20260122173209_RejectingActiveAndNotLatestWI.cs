using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RejectingActiveAndNotLatestWI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix existing invalid rows so the constraint can be added
            migrationBuilder.Sql("""
                                     UPDATE WorkInstructions
                                     SET IsActive = 0
                                     WHERE IsActive = 1 AND IsLatest = 0;
                                 """);

            // Enforce: Active ⇒ Latest
            migrationBuilder.AddCheckConstraint(
                name: "CK_WorkInstructions_ActiveRequiresLatest",
                table: "WorkInstructions",
                sql: "[IsActive] = 0 OR [IsLatest] = 1"
            );

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_WorkInstructions_ActiveRequiresLatest",
                table: "WorkInstructions"
            );
        }
    }
}
