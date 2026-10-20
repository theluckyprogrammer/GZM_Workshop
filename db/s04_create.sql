IF OBJECT_ID(N'dbo.BytomLostItems') IS NULL
CREATE TABLE dbo.[BytomLostItems] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_BytomLostItems] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [EntryDate] date NULL,
    [Id] decimal(10,1) NOT NULL,
    [ItemName] nvarchar(80) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [Year] int NOT NULL,
    CONSTRAINT [UQ_BytomLostItems_Key] UNIQUE ([Id])
);
