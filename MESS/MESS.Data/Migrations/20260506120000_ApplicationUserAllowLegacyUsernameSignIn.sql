-- =============================================================================
-- Adds AllowLegacyUsernameSignIn to AspNetUsers (legacy username-only login flag).
-- PostgreSQL / quoted identifiers per EF Core conventions.
-- =============================================================================

BEGIN;

ALTER TABLE "AspNetUsers"
    ADD COLUMN IF NOT EXISTS "AllowLegacyUsernameSignIn" boolean NOT NULL DEFAULT TRUE;

COMMIT;
