IF OBJECT_ID(N'dbo.KatowiceMemorials') IS NULL
CREATE TABLE dbo.[KatowiceMemorials] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_KatowiceMemorials] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [Street] nvarchar(80) NULL,
    [Description] nvarchar(600) NOT NULL,
    [RecordNumber] int NOT NULL,
    [ObjectType] nvarchar(30) NOT NULL,
    CONSTRAINT [UQ_KatowiceMemorials_Key] UNIQUE ([RecordNumber])
);
