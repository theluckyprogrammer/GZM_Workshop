-- Creates the workshop database if it does not exist yet.
-- Run against the "master" database. @DatabaseName comes from the connection string (default: GzmWorkshop).
IF DB_ID(@DatabaseName) IS NULL
BEGIN
    DECLARE @create nvarchar(300) = N'CREATE DATABASE ' + QUOTENAME(@DatabaseName);
    EXEC (@create);
END
