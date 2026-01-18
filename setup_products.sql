-- Create Products table if not exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
BEGIN
    CREATE TABLE Products (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        Category NVARCHAR(100) NULL,
        Price DECIMAL(18,2) NOT NULL,
        StockQuantity INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Products table created';
END
ELSE
BEGIN
    PRINT 'Products table already exists';
END
GO

-- Insert seed data if table is empty
IF NOT EXISTS (SELECT 1 FROM Products)
BEGIN
    INSERT INTO Products (Id, Name, Description, Category, Price, StockQuantity, IsActive, CreatedAt) VALUES
    ('11111111-1111-1111-1111-111111111111', 'Laptop', 'Laptop Gaming ASUS ROG cu RTX 4070', 'Electronics', 5499.99, 10, 1, GETUTCDATE()),
    ('22222222-2222-2222-2222-222222222222', 'iPhone', 'iPhone 15 Pro Max 256GB', 'Electronics', 6299.00, 25, 1, GETUTCDATE()),
    ('33333333-3333-3333-3333-333333333333', 'Samsung TV', 'Samsung TV 65" OLED 4K', 'Electronics', 4999.99, 15, 1, GETUTCDATE()),
    ('44444444-4444-4444-4444-444444444444', 'Sony Headphones', 'Sony WH-1000XM5 Wireless', 'Audio', 1299.00, 50, 1, GETUTCDATE()),
    ('55555555-5555-5555-5555-555555555555', 'PlayStation 5', 'Consola Sony PS5 Digital', 'Gaming', 2499.00, 20, 1, GETUTCDATE());
    PRINT 'Inserted 5 products';
END
ELSE
BEGIN
    PRINT 'Products already exist';
END
GO

SELECT Name, Price, StockQuantity FROM Products;
GO

