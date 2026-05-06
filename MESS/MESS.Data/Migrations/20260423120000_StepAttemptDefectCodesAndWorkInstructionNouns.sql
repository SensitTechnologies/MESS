-- =============================================================================
-- EF migration equivalent: StepAttemptDefectCodesAndWorkInstructionNouns (Up)
-- Apply in pgAdmin 4: Query Tool -> Paste -> Execute (F5)
-- -----------------------------------------------------------------------------
-- 1) Join table WorkInstructionFailureNouns (FailureNounsId, WorkInstructionsId)
-- 2) Nullable FKs FailureNounId, FailureAdjectiveId on ProductionLogStepAttempts
-- =============================================================================

BEGIN;

CREATE TABLE IF NOT EXISTS "WorkInstructionFailureNouns" (
    "FailureNounsId" integer NOT NULL,
    "WorkInstructionsId" integer NOT NULL,
    CONSTRAINT "PK_WorkInstructionFailureNouns" PRIMARY KEY ("FailureNounsId", "WorkInstructionsId"),
    CONSTRAINT "FK_WorkInstructionFailureNouns_FailureNoun_FailureNounsId" FOREIGN KEY ("FailureNounsId") REFERENCES "FailureNoun" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_WorkInstructionFailureNouns_WorkInstructions_WorkInstructionsId" FOREIGN KEY ("WorkInstructionsId") REFERENCES "WorkInstructions" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_WorkInstructionFailureNouns_WorkInstructionsId"
    ON "WorkInstructionFailureNouns" ("WorkInstructionsId");

ALTER TABLE "ProductionLogStepAttempts"
    ADD COLUMN IF NOT EXISTS "FailureAdjectiveId" integer NULL;

ALTER TABLE "ProductionLogStepAttempts"
    ADD COLUMN IF NOT EXISTS "FailureNounId" integer NULL;

CREATE INDEX IF NOT EXISTS "IX_ProductionLogStepAttempts_FailureAdjectiveId"
    ON "ProductionLogStepAttempts" ("FailureAdjectiveId");

CREATE INDEX IF NOT EXISTS "IX_ProductionLogStepAttempts_FailureNounId"
    ON "ProductionLogStepAttempts" ("FailureNounId");

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_ProductionLogStepAttempts_FailureAdjective_FailureAdjectiveId') THEN
        ALTER TABLE "ProductionLogStepAttempts"
            ADD CONSTRAINT "FK_ProductionLogStepAttempts_FailureAdjective_FailureAdjectiveId"
            FOREIGN KEY ("FailureAdjectiveId") REFERENCES "FailureAdjective" ("Id") ON DELETE SET NULL;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_ProductionLogStepAttempts_FailureNoun_FailureNounId') THEN
        ALTER TABLE "ProductionLogStepAttempts"
            ADD CONSTRAINT "FK_ProductionLogStepAttempts_FailureNoun_FailureNounId"
            FOREIGN KEY ("FailureNounId") REFERENCES "FailureNoun" ("Id") ON DELETE SET NULL;
    END IF;
END$$;

COMMIT;

-- Optional: record migration (only if applying SQL instead of dotnet ef database update)
-- INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
-- VALUES ('20260423120000_StepAttemptDefectCodesAndWorkInstructionNouns', '10.0.3')
-- ON CONFLICT DO NOTHING;
