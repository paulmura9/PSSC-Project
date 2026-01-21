-- Add TrackingNumber column to Invoices table if it doesn't exist
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'invoicing.Invoices') 
    AND name = 'TrackingNumber'
)
BEGIN
    ALTER TABLE [invoicing].[Invoices] 
    ADD [TrackingNumber] nvarchar(100) NULL;
    PRINT 'TrackingNumber column added to invoicing.Invoices';
END
ELSE
BEGIN
    PRINT 'TrackingNumber column already exists';
END
GO

