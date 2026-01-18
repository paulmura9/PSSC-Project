-- Migration: Remove Uses column from Vouchers table
-- MaxUses now represents remaining uses (decremented on each use)
-- Run this on Azure SQL database

-- Drop Uses column if exists
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'ordering' AND TABLE_NAME = 'Vouchers' AND COLUMN_NAME = 'Uses')
BEGIN
    ALTER TABLE [ordering].[Vouchers] DROP COLUMN [Uses];
    PRINT 'Uses column dropped from Vouchers table';
END
ELSE
BEGIN
    PRINT 'Uses column does not exist, skipping';
END
GO

PRINT 'Migration completed successfully';
PRINT 'Note: MaxUses now represents REMAINING uses. It will be decremented on each voucher use.';
PRINT 'NULL MaxUses = unlimited uses';

