-- =============================================================================
-- EF migration equivalent: AddStepNotesConfiguration (Up)
-- Apply in pgAdmin 4: Query Tool -> Paste -> Execute (F5)
-- Targets: PostgreSQL (Npgsql / quoted identifiers like EF Core)
--
-- Adds "NotesConfiguration" to "Steps" (integer, NOT NULL, default 0 = OptionalForFailure)
-- =============================================================================

BEGIN;

ALTER TABLE "Steps"
    ADD COLUMN IF NOT EXISTS "NotesConfiguration" integer NOT NULL DEFAULT 0;

COMMIT;

-- =============================================================================
-- Optional: record migration as applied (only if you use __EFMigrationsHistory
-- and are applying this SQL INSTEAD of `dotnet ef database update`)
-- =============================================================================
-- INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
-- VALUES ('20260420130000_AddStepNotesConfiguration', '10.0.3')
-- ON CONFLICT DO NOTHING;

-- =============================================================================
-- DOWN / rollback (run only if you need to reverse the change)
-- =============================================================================
-- BEGIN;
-- ALTER TABLE "Steps" DROP COLUMN IF EXISTS "NotesConfiguration";
-- COMMIT;
-- DELETE FROM "__EFMigrationsHistory"
-- WHERE "MigrationId" = '20260420130000_AddStepNotesConfiguration';
