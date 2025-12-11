using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MESS.Data.Migrations
{
    /// <inheritdoc />
    public partial class OnePartDefinitionPerPartNode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Add the new column (nullable at first) to the PartNodes table (TPT)
            migrationBuilder.AddColumn<int>(
                name: "PartDefinitionId",
                table: "PartNodes",
                type: "int",
                nullable: true);

            // 2) Data migration: expand PartNodeParts into multiple PartNodes while preserving ordering.
            //    We compute a full ordered list for each WorkInstruction and then:
            //      - update existing rows for non-part nodes and the first part of a part-node
            //      - create new WorkInstructionNodes + PartNodes for extra parts
            //      - drop the join table
            //      - normalize positions to dense 0-based indexes per WorkInstruction
            migrationBuilder.Sql(@"
                PRINT 'Starting OnePartDefinitionPerPartNode migration...';

                -- Temporary table to contain expanded entries
                IF OBJECT_ID('tempdb..#Expanded') IS NOT NULL DROP TABLE #Expanded;
                CREATE TABLE #Expanded
                (
                    WorkInstructionId INT NOT NULL,
                    OriginalPosition INT NOT NULL,
                    OriginalNodeId INT NOT NULL,
                    rn INT NOT NULL,                 -- 0 = non-part or part-node with no parts; 1 = first part; >1 additional parts
                    PartDefinitionId INT NULL
                );

                -- (A) Non-Part nodes (keep as-is)
                INSERT INTO #Expanded (WorkInstructionId, OriginalPosition, OriginalNodeId, rn, PartDefinitionId)
                SELECT wn.WorkInstructionId, wn.Position, wn.Id, 0 AS rn, NULL AS PartDefinitionId
                FROM WorkInstructionNodes wn
                LEFT JOIN PartNodes pn ON pn.Id = wn.Id
                WHERE pn.Id IS NULL;

                -- (B) PartNodes with no entries in join table -> keep as single node (will be deleted later)
                INSERT INTO #Expanded (WorkInstructionId, OriginalPosition, OriginalNodeId, rn, PartDefinitionId)
                SELECT wn.WorkInstructionId, wn.Position, pn.Id, 0 AS rn, NULL AS PartDefinitionId
                FROM PartNodes pn
                JOIN WorkInstructionNodes wn ON wn.Id = pn.Id
                LEFT JOIN PartNodeParts pnp ON pnp.PartNodeId = pn.Id
                WHERE pnp.PartNodeId IS NULL;

                -- (C) Expand part nodes: one row per part with row-number inside that PartNode
                INSERT INTO #Expanded (WorkInstructionId, OriginalPosition, OriginalNodeId, rn, PartDefinitionId)
                SELECT wn.WorkInstructionId, wn.Position, pn.Id,
                       ROW_NUMBER() OVER (PARTITION BY pnp.PartNodeId ORDER BY pnp.PartsId) AS rn,
                       pnp.PartsId AS PartDefinitionId
                FROM PartNodeParts pnp
                JOIN PartNodes pn ON pn.Id = pnp.PartNodeId
                JOIN WorkInstructionNodes wn ON wn.Id = pn.Id;

                -- Compute the new position (0-based) within each WorkInstruction
                IF OBJECT_ID('tempdb..#WithNewPos') IS NOT NULL DROP TABLE #WithNewPos;
                SELECT e.*,
                       (ROW_NUMBER() OVER (PARTITION BY e.WorkInstructionId ORDER BY e.OriginalPosition, e.rn) - 1) AS NewPosition
                INTO #WithNewPos
                FROM #Expanded e;

                -- Update positions of existing WorkInstructionNodes for entries that correspond to:
                --   * non-part nodes (rn = 0)
                --   * first part of a part-node (rn = 1)
                UPDATE win
                SET win.Position = w.NewPosition
                FROM WorkInstructionNodes win
                JOIN #WithNewPos w ON win.Id = w.OriginalNodeId
                WHERE w.rn = 0 OR w.rn = 1;

                -- Set PartDefinitionId on existing PartNodes for rn = 1 (first part)
                UPDATE pn
                SET pn.PartDefinitionId = w.PartDefinitionId
                FROM PartNodes pn
                JOIN #WithNewPos w ON pn.Id = w.OriginalNodeId
                WHERE w.rn = 1 AND w.PartDefinitionId IS NOT NULL;

                -- Insert new WorkInstructionNodes for every extra part (rn > 1)
                -- Capture mapping (inserted.Id -> source row) into #NewNodes.
                PRINT 'Inserting new WorkInstructionNodes for expanded part nodes...';

                -- Create #ToInsert with identity-based ordering for correlation
                IF OBJECT_ID('tempdb..#ToInsert') IS NOT NULL DROP TABLE #ToInsert;
                SELECT 
                    IDENTITY(INT, 1, 1) AS RowNum,
                    WorkInstructionId,
                    NewPosition,
                    PartDefinitionId
                INTO #ToInsert
                FROM #WithNewPos
                WHERE rn > 1
                ORDER BY WorkInstructionId, NewPosition;

                -- Create the destination mapping table
                IF OBJECT_ID('tempdb..#NewNodes') IS NOT NULL DROP TABLE #NewNodes;
                CREATE TABLE #NewNodes
                (
                    RowNum INT NOT NULL,
                    NewNodeId INT NOT NULL,
                    WorkInstructionId INT NOT NULL,
                    NewPosition INT NOT NULL,
                    PartDefinitionId INT NULL
                );

                -- Insert new WorkInstructionNodes and capture generated IDs
                DECLARE @InsertedNodes TABLE (NewNodeId INT);

                INSERT INTO WorkInstructionNodes (WorkInstructionId, Position, NodeType)
                OUTPUT inserted.Id INTO @InsertedNodes
                SELECT WorkInstructionId, NewPosition, 0
                FROM #ToInsert
                ORDER BY RowNum;

                -- Map back to the original source rows using RowNum
                INSERT INTO #NewNodes (RowNum, NewNodeId, WorkInstructionId, NewPosition, PartDefinitionId)
                SELECT t.RowNum, i.NewNodeId, t.WorkInstructionId, t.NewPosition, t.PartDefinitionId
                FROM #ToInsert t
                JOIN (
                    SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS RowNum, NewNodeId
                    FROM @InsertedNodes
                ) i ON i.RowNum = t.RowNum;

                -- Insert corresponding PartNodes rows for newly created nodes
                -- PartNodes TPT table must have (Id, PartDefinitionId) at this point
                INSERT INTO PartNodes (Id, PartDefinitionId)
                SELECT n.NewNodeId, n.PartDefinitionId
                FROM #NewNodes n;

                -- Drop the join table now that data is migrated
                IF OBJECT_ID('dbo.PartNodeParts', 'U') IS NOT NULL
                BEGIN
                    PRINT 'Dropping join table PartNodeParts...';
                    DROP TABLE dbo.PartNodeParts;
                END

                -- Delete any PartNodes that still have NULL PartDefinitionId (these were empty)
                PRINT 'Deleting PartNodes with no part (NULL PartDefinitionId)...';
                DELETE pn
                FROM PartNodes pn
                WHERE pn.PartDefinitionId IS NULL;

                -- Normalize positions to dense 0-based integers per WorkInstruction
                ;WITH Ordered AS
                (
                    SELECT win.Id,
                           ROW_NUMBER() OVER (PARTITION BY win.WorkInstructionId ORDER BY win.Position, win.Id) - 1 AS DensePos
                    FROM WorkInstructionNodes win
                )
                UPDATE win
                SET Position = o.DensePos
                FROM WorkInstructionNodes win
                JOIN Ordered o ON win.Id = o.Id;

                PRINT 'OnePartDefinitionPerPartNode: expansion and normalization complete.';
            ");

            // 3) Make PartDefinitionId non-nullable and add index + FK
            //    (we deleted any nodes with NULL PartDefinitionId above)
            migrationBuilder.Sql(@"
                PRINT 'Checking for NULL PartDefinitionId values before altering column...';

                IF EXISTS (SELECT 1 FROM PartNodes WHERE PartDefinitionId IS NULL)
                BEGIN
                    PRINT 'WARNING: Some PartNodes still have NULL PartDefinitionId — NOT NULL alteration skipped.';
                END
                ELSE
                BEGIN
                    PRINT 'Altering PartNodes.PartDefinitionId to NOT NULL...';
                    ALTER TABLE PartNodes ALTER COLUMN PartDefinitionId INT NOT NULL;
                END
            ");

            migrationBuilder.CreateIndex(
                name: "IX_PartNodes_PartDefinitionId",
                table: "PartNodes",
                column: "PartDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartNodes_PartDefinitions_PartDefinitionId",
                table: "PartNodes",
                column: "PartDefinitionId",
                principalTable: "PartDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("PRINT 'OnePartDefinitionPerPartNode Up() complete.';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Best-effort rollback: recreate join table and repopulate from PartNodes.
            // Note: this will not merge split PartNodes back into single aggregated nodes.
            migrationBuilder.CreateTable(
                name: "PartNodeParts",
                columns: table => new
                {
                    PartNodeId = table.Column<int>(type: "int", nullable: false),
                    PartsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartNodeParts", x => new { x.PartNodeId, x.PartsId });
                    table.ForeignKey(
                        name: "FK_PartNodeParts_PartDefinitions_PartsId",
                        column: x => x.PartsId,
                        principalTable: "PartDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartNodeParts_PartNodes_PartNodeId",
                        column: x => x.PartNodeId,
                        principalTable: "PartNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartNodeParts_PartsId",
                table: "PartNodeParts",
                column: "PartsId");

            // Populate the join table from existing part nodes (one-to-one mapping)
            migrationBuilder.Sql(@"
                PRINT 'Repopulating PartNodeParts from PartNodes (best-effort)...';
                INSERT INTO PartNodeParts (PartNodeId, PartsId)
                SELECT pn.Id, pn.PartDefinitionId
                FROM PartNodes pn;
                PRINT 'PartNodeParts repopulated.';");

            // Drop FK + index and drop the PartDefinitionId column
            migrationBuilder.DropForeignKey(
                name: "FK_PartNodes_PartDefinitions_PartDefinitionId",
                table: "PartNodes");

            migrationBuilder.DropIndex(
                name: "IX_PartNodes_PartDefinitionId",
                table: "PartNodes");

            migrationBuilder.DropColumn(
                name: "PartDefinitionId",
                table: "PartNodes");

            migrationBuilder.Sql("PRINT 'OnePartDefinitionPerPartNode Down() complete.';");
        }
    }
}
