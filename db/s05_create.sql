IF OBJECT_ID(N'dbo.DabrowaLostItems') IS NULL
CREATE TABLE dbo.[DabrowaLostItems] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_DabrowaLostItems] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [CollectionDate] date NOT NULL,
    [FoundDate] date NOT NULL,
    [Id] nvarchar(30) NOT NULL,
    [ItemName] nvarchar(60) NOT NULL,
    CONSTRAINT [UQ_DabrowaLostItems_Key] UNIQUE ([Id])
);
