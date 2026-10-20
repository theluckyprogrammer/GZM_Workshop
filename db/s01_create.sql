IF OBJECT_ID(N'dbo.ZabrzeMemorials') IS NULL
CREATE TABLE dbo.[ZabrzeMemorials] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ZabrzeMemorials] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [City] nvarchar(40) NULL,
    [Street] nvarchar(60) NULL,
    [Description] nvarchar(800) NOT NULL,
    [Inscription] nvarchar(1500) NULL,
    [LocationName] nvarchar(100) NULL,
    [RecordNumber] nvarchar(20) NOT NULL,
    CONSTRAINT [UQ_ZabrzeMemorials_Key] UNIQUE ([RecordNumber])
);
