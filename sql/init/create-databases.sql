IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'AuthDb')
BEGIN
    CREATE DATABASE AuthDb;
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'InventoryDb')
BEGIN
    CREATE DATABASE InventoryDb;
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SalesDb')
BEGIN
    CREATE DATABASE SalesDb;
END
GO
