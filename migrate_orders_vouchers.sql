-- Migration: Update Orders table for voucher support
-- This script updates the schema to use Subtotal, DiscountAmount, Total instead of TotalPrice

-- Step 1: Add new columns if they don't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'ordering' AND TABLE_NAME = 'Orders' AND COLUMN_NAME = 'Subtotal')
BEGIN
    ALTER TABLE [ordering].[Orders] ADD [Subtotal] DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Subtotal column added';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'ordering' AND TABLE_NAME = 'Orders' AND COLUMN_NAME = 'DiscountAmount')
BEGIN
    ALTER TABLE [ordering].[Orders] ADD [DiscountAmount] DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'DiscountAmount column added';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'ordering' AND TABLE_NAME = 'Orders' AND COLUMN_NAME = 'Total')
BEGIN
    ALTER TABLE [ordering].[Orders] ADD [Total] DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Total column added';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'ordering' AND TABLE_NAME = 'Orders' AND COLUMN_NAME = 'VoucherCode')
BEGIN
    ALTER TABLE [ordering].[Orders] ADD [VoucherCode] NVARCHAR(64) NULL;
    PRINT 'VoucherCode column added';
END
GO

-- Step 2: Migrate data from TotalPrice to new columns (if TotalPrice exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'ordering' AND TABLE_NAME = 'Orders' AND COLUMN_NAME = 'TotalPrice')
BEGIN
    UPDATE [ordering].[Orders] 
    SET Subtotal = TotalPrice, 
        Total = TotalPrice,
        DiscountAmount = 0
    WHERE Subtotal = 0 OR Total = 0;
    PRINT 'Data migrated from TotalPrice to Subtotal/Total';
END
GO

-- Step 3: Drop old TotalPrice column (it's now computed as Total)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'ordering' AND TABLE_NAME = 'Orders' AND COLUMN_NAME = 'TotalPrice')
BEGIN
    -- First check if there are any constraints on TotalPrice
    DECLARE @constraintName NVARCHAR(200);
    
    SELECT @constraintName = dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE OBJECT_NAME(dc.parent_object_id) = 'Orders' 
      AND SCHEMA_NAME(SCHEMA_ID()) = 'ordering'
      AND c.name = 'TotalPrice';
    
    IF @constraintName IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE [ordering].[Orders] DROP CONSTRAINT ' + @constraintName);
        PRINT 'Dropped default constraint on TotalPrice';
    END
    
    ALTER TABLE [ordering].[Orders] DROP COLUMN [TotalPrice];
    PRINT 'TotalPrice column dropped';
END
GO

-- Step 4: Create Vouchers table if not exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'ordering' AND TABLE_NAME = 'Vouchers')
BEGIN
    CREATE TABLE [ordering].[Vouchers] (
        [VoucherId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Code] NVARCHAR(64) NOT NULL,
        [DiscountPercent] INT NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [ValidFrom] DATETIME2 NULL,
        [ValidTo] DATETIME2 NULL,
        [MaxUses] INT NULL,
        [Uses] INT NOT NULL DEFAULT 0
    );
    
    CREATE UNIQUE INDEX [IX_Vouchers_Code] ON [ordering].[Vouchers] ([Code]);
    PRINT 'Vouchers table created';
END
GO

-- Step 5: Seed example vouchers
IF NOT EXISTS (SELECT 1 FROM [ordering].[Vouchers] WHERE Code = 'WELCOME10')
BEGIN
    INSERT INTO [ordering].[Vouchers] (VoucherId, Code, DiscountPercent, IsActive, ValidFrom, ValidTo, MaxUses, Uses)
    VALUES (NEWID(), 'WELCOME10', 10, 1, NULL, NULL, NULL, 0);
    PRINT 'WELCOME10 voucher created';
END

IF NOT EXISTS (SELECT 1 FROM [ordering].[Vouchers] WHERE Code = 'SUMMER20')
BEGIN
    INSERT INTO [ordering].[Vouchers] (VoucherId, Code, DiscountPercent, IsActive, ValidFrom, ValidTo, MaxUses, Uses)
    VALUES (NEWID(), 'SUMMER20', 20, 1, '2026-06-01', '2026-08-31', 100, 0);
    PRINT 'SUMMER20 voucher created';
END

IF NOT EXISTS (SELECT 1 FROM [ordering].[Vouchers] WHERE Code = 'VIP50')
BEGIN
    INSERT INTO [ordering].[Vouchers] (VoucherId, Code, DiscountPercent, IsActive, ValidFrom, ValidTo, MaxUses, Uses)
    VALUES (NEWID(), 'VIP50', 50, 1, NULL, NULL, 10, 0);
    PRINT 'VIP50 voucher created';
END
GO

PRINT 'Migration completed successfully';

