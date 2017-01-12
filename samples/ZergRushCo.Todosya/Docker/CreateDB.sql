CREATE LOGIN TestUser WITH PASSWORD = 'gHJT2SCm';
GO

CREATE DATABASE ZergRushCo;
GO

USE ZergRushCo
GO

CREATE USER TestUser FOR LOGIN TestUser;
-- GRANT ALTER, REFERENCES TO TestUser;
EXEC sp_addrolemember N'db_owner', N'TestUser';
GO
