-- Clean up duplicate invoice that caused the error
-- Run with: sqlcmd -S "pssc-server.database.windows.net" -d "pssc-db" -U "CloudSAa6035330" -P "pssc2026!" -i "cleanup_duplicate_invoice.sql"

-- Delete the duplicate invoice lines first (foreign key constraint)
DELETE FROM [invoicing].[InvoiceLines] 
WHERE InvoiceId = '8ed4c3e1-4080-40b5-807b-4d02274b5bc4';

-- Delete the duplicate invoice
DELETE FROM [invoicing].[Invoices] 
WHERE InvoiceId = '8ed4c3e1-4080-40b5-807b-4d02274b5bc4';

PRINT 'Cleanup complete';

