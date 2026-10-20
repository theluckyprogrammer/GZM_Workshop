IF OBJECT_ID(N'dbo.ArchaeologicalSites') IS NULL
CREATE TABLE dbo.[ArchaeologicalSites] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ArchaeologicalSites] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [AzpArea] nvarchar(10) NOT NULL,
    [SiteNumberInCity] int NOT NULL,
    [AzpNumber] int NOT NULL,
    [Chronology] nvarchar(200) NULL,
    [Comment] nvarchar(400) NULL,
    [Id] int NOT NULL,
    [LocalName] nvarchar(200) NOT NULL,
    CONSTRAINT [UQ_ArchaeologicalSites_Key] UNIQUE ([Id])
);
