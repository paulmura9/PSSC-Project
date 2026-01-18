-- Drop Uses column from Vouchers table
ALTER TABLE [ordering].[Vouchers] DROP COLUMN IF EXISTS [Uses];
GO
PRINT 'Uses column dropped successfully';
GO

