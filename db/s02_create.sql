IF OBJECT_ID(N'dbo.BikeStations') IS NULL
CREATE TABLE dbo.[BikeStations] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_BikeStations] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [Name] nvarchar(20) NOT NULL,
    [Number] int NOT NULL,
    [Area] nvarchar(50) NOT NULL,
    [BaseBikes] int NOT NULL,
    [Racks] int NOT NULL,
    [Available] bit NOT NULL,
    [Lat] decimal(12,9) NOT NULL,
    [Lng] decimal(12,9) NOT NULL,
    CONSTRAINT [UQ_BikeStations_Key] UNIQUE ([Number])
);
