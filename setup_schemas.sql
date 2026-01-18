-- Create schemas for bounded contexts isolation
-- Run this script once on the database

-- Create schemas if they don't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'ordering')
BEGIN
    EXEC('CREATE SCHEMA ordering');
    PRINT 'Schema [ordering] created';
END
ELSE
    PRINT 'Schema [ordering] already exists';

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'shipment')
BEGIN
    EXEC('CREATE SCHEMA shipment');
    PRINT 'Schema [shipment] created';
END
ELSE
    PRINT 'Schema [shipment] already exists';

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'invoicing')
BEGIN
    EXEC('CREATE SCHEMA invoicing');
    PRINT 'Schema [invoicing] created';
END
ELSE
    PRINT 'Schema [invoicing] already exists';

GO

-- =============================================
-- ORDERING SCHEMA TABLES
-- =============================================

-- Products table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'ordering' AND TABLE_NAME = 'Products')
BEGIN
    CREATE TABLE ordering.Products (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        Category NVARCHAR(100) NULL,
        Price DECIMAL(18,2) NOT NULL,
        StockQuantity INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL
    );
    PRINT 'Table [ordering].[Products] created';
END

-- Orders table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'ordering' AND TABLE_NAME = 'Orders')
BEGIN
    CREATE TABLE ordering.Orders (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        Street NVARCHAR(200) NOT NULL,
        City NVARCHAR(100) NOT NULL,
        PostalCode NVARCHAR(10) NOT NULL,
        Phone NVARCHAR(20) NOT NULL,
        Email NVARCHAR(254) NULL,
        DeliveryNotes NVARCHAR(250) NULL,
        TotalPrice DECIMAL(18,2) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL
    );
    PRINT 'Table [ordering].[Orders] created';
END

-- OrderLines table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'ordering' AND TABLE_NAME = 'OrderLines')
BEGIN
    CREATE TABLE ordering.OrderLines (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        OrderId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        Category NVARCHAR(100) NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        LineTotal DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_OrderLines_Orders FOREIGN KEY (OrderId) REFERENCES ordering.Orders(Id) ON DELETE CASCADE
    );
    PRINT 'Table [ordering].[OrderLines] created';
END

GO

-- =============================================
-- SHIPMENT SCHEMA TABLES
-- =============================================

-- Shipments table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'shipment' AND TABLE_NAME = 'Shipments')
BEGIN
    CREATE TABLE shipment.Shipments (
        ShipmentId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        OrderId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        TotalPrice DECIMAL(18,2) NOT NULL,
        TrackingNumber NVARCHAR(100) NOT NULL,
        Status NVARCHAR(50) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL
    );
    
    CREATE UNIQUE INDEX IX_Shipments_TrackingNumber ON shipment.Shipments(TrackingNumber);
    CREATE INDEX IX_Shipments_OrderId ON shipment.Shipments(OrderId);
    
    PRINT 'Table [shipment].[Shipments] created';
END

-- ShipmentLines table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'shipment' AND TABLE_NAME = 'ShipmentLines')
BEGIN
    CREATE TABLE shipment.ShipmentLines (
        ShipmentLineId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ShipmentId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        Category NVARCHAR(100) NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        LineTotal DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_ShipmentLines_Shipments FOREIGN KEY (ShipmentId) REFERENCES shipment.Shipments(ShipmentId) ON DELETE CASCADE
    );
    PRINT 'Table [shipment].[ShipmentLines] created';
END

GO

-- =============================================
-- INVOICING SCHEMA TABLES
-- =============================================

-- Invoices table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'invoicing' AND TABLE_NAME = 'Invoices')
BEGIN
    CREATE TABLE invoicing.Invoices (
        InvoiceId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        InvoiceNumber NVARCHAR(50) NOT NULL,
        ShipmentId UNIQUEIDENTIFIER NOT NULL,
        OrderId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        TrackingNumber NVARCHAR(100) NULL,
        SubTotal DECIMAL(18,2) NOT NULL,
        Tax DECIMAL(18,2) NOT NULL,
        TotalAmount DECIMAL(18,2) NOT NULL,
        Status NVARCHAR(50) NOT NULL,
        InvoiceDate DATETIME2 NOT NULL,
        DueDate DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL
    );
    
    CREATE UNIQUE INDEX IX_Invoices_InvoiceNumber ON invoicing.Invoices(InvoiceNumber);
    CREATE INDEX IX_Invoices_OrderId ON invoicing.Invoices(OrderId);
    
    PRINT 'Table [invoicing].[Invoices] created';
END

-- InvoiceLines table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'invoicing' AND TABLE_NAME = 'InvoiceLines')
BEGIN
    CREATE TABLE invoicing.InvoiceLines (
        InvoiceLineId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        InvoiceId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        Category NVARCHAR(100) NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        LineTotal DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_InvoiceLines_Invoices FOREIGN KEY (InvoiceId) REFERENCES invoicing.Invoices(InvoiceId) ON DELETE CASCADE
    );
    PRINT 'Table [invoicing].[InvoiceLines] created';
END

GO

-- Seed some products for testing
IF NOT EXISTS (SELECT * FROM ordering.Products)
BEGIN
    INSERT INTO ordering.Products (Id, Name, Description, Category, Price, StockQuantity, CreatedAt)
    VALUES 
        (NEWID(), 'Laptop', 'High-performance laptop', 'Electronics', 2500.00, 50, GETUTCDATE()),
        (NEWID(), 'Mouse', 'Wireless mouse', 'Electronics', 50.00, 200, GETUTCDATE()),
        (NEWID(), 'Keyboard', 'Mechanical keyboard', 'Electronics', 150.00, 100, GETUTCDATE()),
        (NEWID(), 'Monitor', '27 inch 4K monitor', 'Electronics', 800.00, 30, GETUTCDATE()),
        (NEWID(), 'Headphones', 'Noise-cancelling headphones', 'Electronics', 300.00, 75, GETUTCDATE());
    
    PRINT 'Sample products inserted into [ordering].[Products]';
END

GO

PRINT 'Database schema setup completed successfully!';

