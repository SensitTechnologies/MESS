using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSerializablePart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogs_Products_ProductId",
                table: "ProductionLogs");

            migrationBuilder.AlterColumn<int>(
                name: "WorkInstructionId",
                table: "ProductionLogs",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "ProductionLogs",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SerializablePartId",
                table: "ProductionLogParts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SerializableParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartId = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerializableParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SerializableParts_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SerializableParts_SerializableParts_ParentId",
                        column: x => x.ParentId,
                        principalTable: "SerializableParts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogParts_SerializablePartId",
                table: "ProductionLogParts",
                column: "SerializablePartId");

            migrationBuilder.CreateIndex(
                name: "IX_SerializableParts_ParentId",
                table: "SerializableParts",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_SerializableParts_PartId",
                table: "SerializableParts",
                column: "PartId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogParts_SerializableParts_SerializablePartId",
                table: "ProductionLogParts",
                column: "SerializablePartId",
                principalTable: "SerializableParts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogs_Products_ProductId",
                table: "ProductionLogs",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogParts_SerializableParts_SerializablePartId",
                table: "ProductionLogParts");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogs_Products_ProductId",
                table: "ProductionLogs");

            migrationBuilder.DropTable(
                name: "SerializableParts");

            migrationBuilder.DropIndex(
                name: "IX_ProductionLogParts_SerializablePartId",
                table: "ProductionLogParts");

            migrationBuilder.DropColumn(
                name: "SerializablePartId",
                table: "ProductionLogParts");

            migrationBuilder.AlterColumn<int>(
                name: "WorkInstructionId",
                table: "ProductionLogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "ProductionLogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogs_Products_ProductId",
                table: "ProductionLogs",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
