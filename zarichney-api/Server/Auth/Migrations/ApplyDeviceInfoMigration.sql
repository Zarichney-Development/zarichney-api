-- Add new columns to the RefreshTokens table
ALTER TABLE public."RefreshTokens" 
ADD COLUMN IF NOT EXISTS "DeviceName" text NULL,
ADD COLUMN IF NOT EXISTS "DeviceIp" text NULL,
ADD COLUMN IF NOT EXISTS "UserAgent" text NULL,
ADD COLUMN IF NOT EXISTS "LastUsedAt" timestamp NULL;

-- Update migration history
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250324000000_AddDeviceInfoToRefreshTokens', '8.0.14')
ON CONFLICT ("MigrationId") DO NOTHING;