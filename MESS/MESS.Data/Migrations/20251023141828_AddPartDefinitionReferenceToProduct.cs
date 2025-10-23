using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPartDefinitionReferenceToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add new column (nullable for now so we can populate it)
            migrationBuilder.AddColumn<int>(
                name: "PartDefinitionId",
                table: "Products",
                type: "int",
                nullable: true);

            // 2. Create new PartDefinitions for all existing Products
            // We temporarily reuse Product.Name for both Number and Name
            migrationBuilder.Sql(@"
                INSERT INTO PartDefinitions (Number, Name)
                SELECT 
                    'AUTO-' + CAST(Id AS NVARCHAR(10)) AS Number,
                    Name
                FROM Products
            ");

            // 3. Update Product.PartDefinitionId to reference the new PartDefinitions
            migrationBuilder.Sql(@"
                UPDATE p
                SET p.PartDefinitionId = pd.Id
                FROM Products p
                INNER JOIN PartDefinitions pd
                    ON pd.Number = 'AUTO-' + CAST(p.Id AS NVARCHAR(10))
            ");

            // 4. Now that all products have PartDefinitionId, make it required
            migrationBuilder.AlterColumn<int>(
                name: "PartDefinitionId",
                table: "Products",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // 5. Drop old Name column (after we’ve migrated data)
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Products");

            // 6. Add index + foreign key
            migrationBuilder.CreateIndex(
                name: "IX_Products_PartDefinitionId",
                table: "Products",
                column: "PartDefinitionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_PartDefinitions_PartDefinitionId",
                table: "Products",
                column: "PartDefinitionId",
                principalTable: "PartDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_PartDefinitions_PartDefinitionId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_PartDefinitionId",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Restore Product.Name values from associated PartDefinitions
            migrationBuilder.Sql(@"
                UPDATE p
                SET p.Name = pd.Name
                FROM Products p
                INNER JOIN PartDefinitions pd 
                    ON p.PartDefinitionId = pd.Id
            ");

            migrationBuilder.DropColumn(
                name: "PartDefinitionId",
                table: "Products");
        }
    }
}
