-- Migration: Add Vouchers table and update Orders table for voucher support
-- Run this on Azure SQL database

-- Create Vouchers table
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
    
    -- Create unique index on Code
    CREATE UNIQUE INDEX [IX_Vouchers_Code] ON [ordering].[Vouchers] ([Code]);
    
    PRINT 'Vouchers table created';
END
ELSE
    PRINT 'Vouchers table already exists';
GO

-- Add voucher-related columns to Orders table
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

-- Migrate existing data: copy TotalPrice to Subtotal and Total
UPDATE [ordering].[Orders] 
SET Subtotal = TotalPrice, Total = TotalPrice 
WHERE Subtotal = 0 AND TotalPrice > 0;
GO

-- Seed example voucher: WELCOME10 (10% discount)
IF NOT EXISTS (SELECT 1 FROM [ordering].[Vouchers] WHERE Code = 'WELCOME10')
BEGIN
    INSERT INTO [ordering].[Vouchers] (VoucherId, Code, DiscountPercent, IsActive, ValidFrom, ValidTo, MaxUses, Uses)
    VALUES (NEWID(), 'WELCOME10', 10, 1, NULL, NULL, NULL, 0);
    PRINT 'WELCOME10 voucher created';
END
GO

-- Add more example vouchers
IF NOT EXISTS (SELECT 1 FROM [ordering].[Vouchers] WHERE Code = 'SUMMER20')
BEGIN
    INSERT INTO [ordering].[Vouchers] (VoucherId, Code, DiscountPercent, IsActive, ValidFrom, ValidTo, MaxUses, Uses)
    VALUES (NEWID(), 'SUMMER20', 20, 1, '2026-06-01', '2026-08-31', 100, 0);
    PRINT 'SUMMER20 voucher created (20% discount, valid Jun-Aug 2026, max 100 uses)';
END

IF NOT EXISTS (SELECT 1 FROM [ordering].[Vouchers] WHERE Code = 'VIP50')
BEGIN
    INSERT INTO [ordering].[Vouchers] (VoucherId, Code, DiscountPercent, IsActive, ValidFrom, ValidTo, MaxUses, Uses)
    VALUES (NEWID(), 'VIP50', 50, 1, NULL, NULL, 10, 0);
    PRINT 'VIP50 voucher created (50% discount, max 10 uses)';
END

IF NOT EXISTS (SELECT 1 FROM [ordering].[Vouchers] WHERE Code = 'EXPIRED5')
BEGIN
    INSERT INTO [ordering].[Vouchers] (VoucherId, Code, DiscountPercent, IsActive, ValidFrom, ValidTo, MaxUses, Uses)
    VALUES (NEWID(), 'EXPIRED5', 5, 1, '2025-01-01', '2025-12-31', NULL, 0);
    PRINT 'EXPIRED5 voucher created (5% discount, expired - for testing)';
END

IF NOT EXISTS (SELECT 1 FROM [ordering].[Vouchers] WHERE Code = 'INACTIVE15')
BEGIN
    INSERT INTO [ordering].[Vouchers] (VoucherId, Code, DiscountPercent, IsActive, ValidFrom, ValidTo, MaxUses, Uses)
    VALUES (NEWID(), 'INACTIVE15', 15, 0, NULL, NULL, NULL, 0);
    PRINT 'INACTIVE15 voucher created (15% discount, inactive - for testing)';
END
GO

PRINT 'Migration completed successfully';

