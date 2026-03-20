using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class TagsAndTagHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SerializablePartId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_SerializableParts_SerializablePartId",
                        column: x => x.SerializablePartId,
                        principalTable: "SerializableParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TagHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TagId = table.Column<int>(type: "integer", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SerializablePartId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagHistories_SerializableParts_SerializablePartId",
                        column: x => x.SerializablePartId,
                        principalTable: "SerializableParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TagHistories_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TagHistories_SerializablePartId",
                table: "TagHistories",
                column: "SerializablePartId");

            migrationBuilder.CreateIndex(
                name: "IX_TagHistories_TagId",
                table: "TagHistories",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Code",
                table: "Tags",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_SerializablePartId",
                table: "Tags",
                column: "SerializablePartId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagHistories");

            migrationBuilder.DropTable(
                name: "Tags");
        }
    }
}
