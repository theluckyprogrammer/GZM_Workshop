IF OBJECT_ID(N'dbo.HistoricBuildings') IS NULL
CREATE TABLE dbo.[HistoricBuildings] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_HistoricBuildings] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [BuildingNumber] nvarchar(10) NULL,
    [Street] nvarchar(100) NULL,
    [Id] int NOT NULL,
    [Name] nvarchar(1000) NOT NULL,
    [Style] nvarchar(80) NULL,
    CONSTRAINT [UQ_HistoricBuildings_Key] UNIQUE ([Id])
);
