IF OBJECT_ID(N'dbo.SportsFields') IS NULL
CREATE TABLE dbo.[SportsFields] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_SportsFields] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [Fid] int NOT NULL,
    [Kind] nvarchar(80) NOT NULL,
    [Name] nvarchar(80) NULL,
    [Address] nvarchar(50) NOT NULL,
    [X] decimal(20,10) NOT NULL,
    [Y] decimal(20,10) NOT NULL,
    CONSTRAINT [UQ_SportsFields_Key] UNIQUE ([Fid])
);
