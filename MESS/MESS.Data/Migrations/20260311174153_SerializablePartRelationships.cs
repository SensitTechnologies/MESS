using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class SerializablePartRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SerializablePartRelationship",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChildPartId = table.Column<int>(type: "integer", nullable: false),
                    ParentPartId = table.Column<int>(type: "integer", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_SerializablePartRelationship_ChildPartId",
                table: "SerializablePartRelationship",
                column: "ChildPartId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SerializablePartRelationship_ParentPartId",
                table: "SerializablePartRelationship",
                column: "ParentPartId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SerializablePartRelationship");
        }
    }
}
