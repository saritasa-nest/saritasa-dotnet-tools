CREATE LOGIN TestUser WITH PASSWORD = 'gHJT2SCm';
GO

CREATE DATABASE BoringWarehouse;
GO

USE BoringWarehouse
GO

CREATE USER TestUser FOR LOGIN TestUser;
-- GRANT ALTER, REFERENCES TO TestUser;
EXEC sp_addrolemember N'db_owner', N'TestUser';
GO
