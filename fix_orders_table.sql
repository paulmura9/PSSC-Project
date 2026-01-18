-- Fix Orders table: Remove old DeliveryAddress column, make new columns work
-- Run this script to fix the database schema

-- Step 1: Drop DeliveryAddress column if exists (it's no longer used)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'DeliveryAddress')
BEGIN
    -- First update any NULL values in new columns from old column
    UPDATE Orders SET Street = DeliveryAddress WHERE Street IS NULL AND DeliveryAddress IS NOT NULL;
    UPDATE Orders SET City = 'Unknown' WHERE City IS NULL;
    
    -- Drop the old column
    ALTER TABLE Orders DROP COLUMN DeliveryAddress;
    PRINT 'Dropped DeliveryAddress column';
END
GO

-- Step 2: Drop CardNumberMasked column if exists (no longer used)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'CardNumberMasked')
BEGIN
    ALTER TABLE Orders DROP COLUMN CardNumberMasked;
    PRINT 'Dropped CardNumberMasked column';
END
GO

-- Step 3: Make sure Street column exists and is NOT NULL
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'Street')
BEGIN
    ALTER TABLE Orders ADD Street NVARCHAR(200) NOT NULL DEFAULT 'Unknown';
    PRINT 'Added Street column';
END
GO

-- Step 4: Make sure City column exists and is NOT NULL
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'City')
BEGIN
    ALTER TABLE Orders ADD City NVARCHAR(100) NOT NULL DEFAULT 'Unknown';
    PRINT 'Added City column';
END
GO

-- Step 5: Make sure Email column exists (nullable)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'Email')
BEGIN
    ALTER TABLE Orders ADD Email NVARCHAR(254) NULL;
    PRINT 'Added Email column';
END
GO

-- Step 6: Make sure DeliveryNotes column exists (nullable)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'DeliveryNotes')
BEGIN
    ALTER TABLE Orders ADD DeliveryNotes NVARCHAR(250) NULL;
    PRINT 'Added DeliveryNotes column';
END
GO

-- Verify the final structure
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Orders'
ORDER BY ORDINAL_POSITION;
GO

PRINT 'Database schema updated successfully!';
GO

