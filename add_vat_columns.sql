-- Add VAT columns to InvoiceLines table
-- Run with: sqlcmd -S "pssc-server.database.windows.net" -d "pssc-db" -U "CloudSAa6035330" -P "pssc2026!" -i "add_vat_columns.sql"

-- Check if columns exist, if not add them
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[invoicing].[InvoiceLines]') AND name = 'VatRate')
BEGIN
    ALTER TABLE [invoicing].[InvoiceLines]
    ADD [VatRate] decimal(18,4) NOT NULL DEFAULT 0.21;
    PRINT 'Added VatRate column';
END
ELSE
BEGIN
    PRINT 'VatRate column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[invoicing].[InvoiceLines]') AND name = 'VatAmount')
BEGIN
    ALTER TABLE [invoicing].[InvoiceLines]
    ADD [VatAmount] decimal(18,2) NOT NULL DEFAULT 0;
    PRINT 'Added VatAmount column';
END
ELSE
BEGIN
    PRINT 'VatAmount column already exists';
END

PRINT 'Migration complete';

