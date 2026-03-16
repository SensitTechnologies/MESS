using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class FailureOptionsAndTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FailureAdjective",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailureAdjective", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FailureNoun",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailureNoun", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SerializablePartRelationship",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChildPartId = table.Column<int>(type: "int", nullable: false),
                    ParentPartId = table.Column<int>(type: "int", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerializablePartRelationship", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SerializablePartRelationship_SerializableParts_ChildPartId",
                        column: x => x.ChildPartId,
                        principalTable: "SerializableParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SerializablePartRelationship_SerializableParts_ParentPartId",
                        column: x => x.ParentPartId,
                        principalTable: "SerializableParts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SerializablePartId = table.Column<int>(type: "int", nullable: true)
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
                name: "FailureNounAdjectives",
                columns: table => new
                {
                    AdjectivesId = table.Column<int>(type: "int", nullable: false),
                    NounsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailureNounAdjectives", x => new { x.AdjectivesId, x.NounsId });
                    table.ForeignKey(
                        name: "FK_FailureNounAdjectives_FailureAdjective_AdjectivesId",
                        column: x => x.AdjectivesId,
                        principalTable: "FailureAdjective",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FailureNounAdjectives_FailureNoun_NounsId",
                        column: x => x.NounsId,
                        principalTable: "FailureNoun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TagId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SerializablePartId = table.Column<int>(type: "int", nullable: true)
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
                name: "IX_FailureNounAdjectives_NounsId",
                table: "FailureNounAdjectives",
                column: "NounsId");

            migrationBuilder.CreateIndex(
                name: "IX_SerializablePartRelationship_ChildPartId",
                table: "SerializablePartRelationship",
                column: "ChildPartId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SerializablePartRelationship_ParentPartId",
                table: "SerializablePartRelationship",
                column: "ParentPartId");

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
                name: "FailureNounAdjectives");

            migrationBuilder.DropTable(
                name: "SerializablePartRelationship");

            migrationBuilder.DropTable(
                name: "TagHistories");

            migrationBuilder.DropTable(
                name: "FailureAdjective");

            migrationBuilder.DropTable(
                name: "FailureNoun");

            migrationBuilder.DropTable(
                name: "Tags");
        }
    }
}
