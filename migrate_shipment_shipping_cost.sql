-- Migration: Add ShippingCost and TotalWithShipping columns to Shipments table
-- Run this on Azure SQL database

-- Add ShippingCost column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'shipment' AND TABLE_NAME = 'Shipments' AND COLUMN_NAME = 'ShippingCost')
BEGIN
    ALTER TABLE [shipment].[Shipments] ADD [ShippingCost] DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'ShippingCost column added';
END

-- Add TotalWithShipping column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'shipment' AND TABLE_NAME = 'Shipments' AND COLUMN_NAME = 'TotalWithShipping')
BEGIN
    ALTER TABLE [shipment].[Shipments] ADD [TotalWithShipping] DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'TotalWithShipping column added';
END
GO

-- Migrate existing data: set TotalWithShipping = TotalPrice for existing records
UPDATE [shipment].[Shipments] 
SET TotalWithShipping = TotalPrice 
WHERE TotalWithShipping = 0 AND TotalPrice > 0;
GO

PRINT 'Migration completed successfully';

