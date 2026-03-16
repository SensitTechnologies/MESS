using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDivWrappersFromSteps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove wrapping <div> ... </div> from Body field
            migrationBuilder.Sql(@"
                UPDATE Steps
                SET Body = SUBSTRING(Body, 6, LEN(Body) - 11)
                WHERE Body LIKE '<div>%</div>';
            ");

            // Remove wrapping <div> ... </div> from Name field
            migrationBuilder.Sql(@"
                UPDATE Steps
                SET Name = SUBSTRING(Name, 6, LEN(Name) - 11)
                WHERE Name LIKE '<div>%</div>';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
