-- Migration: Add Pickup and Payment fields to Orders table
-- Run this migration to add PickupMethod, PickupPointId, PaymentMethod, PaymentStatus columns
-- Also makes address columns (Street, City, PostalCode) nullable for pickup orders

PRINT 'Starting migration: AddPickupAndPaymentFieldsToOrders';

-- Step 1: Make address columns nullable (for pickup orders that don't need address)
BEGIN TRY
    ALTER TABLE [ordering].[Orders] ALTER COLUMN [Street] NVARCHAR(200) NULL;
    PRINT 'Made Street column nullable';
END TRY
BEGIN CATCH
    PRINT 'Street column already nullable or does not exist';
END CATCH

BEGIN TRY
    ALTER TABLE [ordering].[Orders] ALTER COLUMN [City] NVARCHAR(100) NULL;
    PRINT 'Made City column nullable';
END TRY
BEGIN CATCH
    PRINT 'City column already nullable or does not exist';
END CATCH

BEGIN TRY
    ALTER TABLE [ordering].[Orders] ALTER COLUMN [PostalCode] NVARCHAR(10) NULL;
    PRINT 'Made PostalCode column nullable';
END TRY
BEGIN CATCH
    PRINT 'PostalCode column already nullable or does not exist';
END CATCH

-- Step 2: Add PickupMethod column (default to HomeDelivery for existing orders)
IF COL_LENGTH('ordering.Orders', 'PickupMethod') IS NULL
BEGIN
    ALTER TABLE [ordering].[Orders] 
    ADD [PickupMethod] NVARCHAR(32) NOT NULL CONSTRAINT DF_Orders_PickupMethod DEFAULT 'HomeDelivery';
    PRINT 'Added PickupMethod column';
END
ELSE
    PRINT 'PickupMethod column already exists';

-- Step 3: Add PickupPointId column (nullable)
IF COL_LENGTH('ordering.Orders', 'PickupPointId') IS NULL
BEGIN
    ALTER TABLE [ordering].[Orders] 
    ADD [PickupPointId] NVARCHAR(64) NULL;
    PRINT 'Added PickupPointId column';
END
ELSE
    PRINT 'PickupPointId column already exists';

-- Step 4: Add PaymentMethod column (default to CashOnDelivery for existing orders)
IF COL_LENGTH('ordering.Orders', 'PaymentMethod') IS NULL
BEGIN
    ALTER TABLE [ordering].[Orders] 
    ADD [PaymentMethod] NVARCHAR(32) NOT NULL CONSTRAINT DF_Orders_PaymentMethod DEFAULT 'CashOnDelivery';
    PRINT 'Added PaymentMethod column';
END
ELSE
    PRINT 'PaymentMethod column already exists';

-- Step 5: Add PaymentStatus column (default to Pending for existing orders)
IF COL_LENGTH('ordering.Orders', 'PaymentStatus') IS NULL
BEGIN
    ALTER TABLE [ordering].[Orders] 
    ADD [PaymentStatus] NVARCHAR(32) NOT NULL CONSTRAINT DF_Orders_PaymentStatus DEFAULT 'Pending';
    PRINT 'Added PaymentStatus column';
END
ELSE
    PRINT 'PaymentStatus column already exists';

-- Step 6: Add check constraints for valid values (drop first if exist)
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Orders_PickupMethod')
BEGIN
    ALTER TABLE [ordering].[Orders] DROP CONSTRAINT [CK_Orders_PickupMethod];
END

ALTER TABLE [ordering].[Orders]
ADD CONSTRAINT [CK_Orders_PickupMethod] 
CHECK ([PickupMethod] IN ('HomeDelivery', 'EasyBoxPickup', 'PostOfficePickup'));
PRINT 'Added CK_Orders_PickupMethod constraint';

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Orders_PaymentMethod')
BEGIN
    ALTER TABLE [ordering].[Orders] DROP CONSTRAINT [CK_Orders_PaymentMethod];
END

ALTER TABLE [ordering].[Orders]
ADD CONSTRAINT [CK_Orders_PaymentMethod] 
CHECK ([PaymentMethod] IN ('CashOnDelivery', 'CardOnDelivery', 'CardOnline'));
PRINT 'Added CK_Orders_PaymentMethod constraint';

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Orders_PaymentStatus')
BEGIN
    ALTER TABLE [ordering].[Orders] DROP CONSTRAINT [CK_Orders_PaymentStatus];
END

ALTER TABLE [ordering].[Orders]
ADD CONSTRAINT [CK_Orders_PaymentStatus] 
CHECK ([PaymentStatus] IN ('Pending', 'Authorized', 'Failed'));
PRINT 'Added CK_Orders_PaymentStatus constraint';

PRINT 'Migration completed successfully: AddPickupAndPaymentFieldsToOrders';
GO

