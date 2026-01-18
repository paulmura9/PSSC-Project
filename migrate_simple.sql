-- Simplified Migration: Add Pickup and Payment fields to Orders table
-- Copy and run this in Azure Data Studio or SSMS

-- Add PickupMethod column
IF COL_LENGTH('ordering.Orders', 'PickupMethod') IS NULL
    ALTER TABLE [ordering].[Orders] ADD [PickupMethod] NVARCHAR(32) NOT NULL DEFAULT 'HomeDelivery';

-- Add PickupPointId column  
IF COL_LENGTH('ordering.Orders', 'PickupPointId') IS NULL
    ALTER TABLE [ordering].[Orders] ADD [PickupPointId] NVARCHAR(64) NULL;

-- Add PaymentMethod column
IF COL_LENGTH('ordering.Orders', 'PaymentMethod') IS NULL
    ALTER TABLE [ordering].[Orders] ADD [PaymentMethod] NVARCHAR(32) NOT NULL DEFAULT 'CashOnDelivery';

-- Add PaymentStatus column
IF COL_LENGTH('ordering.Orders', 'PaymentStatus') IS NULL
    ALTER TABLE [ordering].[Orders] ADD [PaymentStatus] NVARCHAR(32) NOT NULL DEFAULT 'Pending';

-- Verify columns were added
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'ordering' AND TABLE_NAME = 'Orders'
ORDER BY ORDINAL_POSITION;

