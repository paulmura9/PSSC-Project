-- Migration: Remove card columns, add Street/City/DeliveryNotes to Orders table
-- Run this on the database to update the schema

-- Step 1: Add new columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'Street')
BEGIN
    ALTER TABLE Orders ADD Street NVARCHAR(200) NULL;
    PRINT 'Added Street column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'City')
BEGIN
    ALTER TABLE Orders ADD City NVARCHAR(100) NULL;
    PRINT 'Added City column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'DeliveryNotes')
BEGIN
    ALTER TABLE Orders ADD DeliveryNotes NVARCHAR(250) NULL;
    PRINT 'Added DeliveryNotes column';
END
GO

-- Step 2: Migrate data from DeliveryAddress to Street (if DeliveryAddress exists)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'DeliveryAddress')
    AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'Street')
BEGIN
    UPDATE Orders SET Street = DeliveryAddress WHERE Street IS NULL;
    UPDATE Orders SET City = 'Unknown' WHERE City IS NULL;
    PRINT 'Migrated DeliveryAddress data to Street';
END
GO

-- Step 3: Set default values for NULL fields
UPDATE Orders SET Street = 'Unknown' WHERE Street IS NULL;
UPDATE Orders SET City = 'Unknown' WHERE City IS NULL;
PRINT 'Updated NULL values';
GO

-- Step 4: Drop old columns (uncomment when ready)
-- IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'DeliveryAddress')
-- BEGIN
--     ALTER TABLE Orders DROP COLUMN DeliveryAddress;
--     PRINT 'Dropped DeliveryAddress column';
-- END
-- GO

-- IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'CardNumberMasked')
-- BEGIN
--     ALTER TABLE Orders DROP COLUMN CardNumberMasked;
--     PRINT 'Dropped CardNumberMasked column';
-- END
-- GO

PRINT 'Migration completed';
GO

