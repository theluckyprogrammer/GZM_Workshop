IF OBJECT_ID(N'dbo.NatureMonuments') IS NULL
CREATE TABLE dbo.[NatureMonuments] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_NatureMonuments] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [CadastralDistrict] nvarchar(50) NOT NULL,
    [Circumference] int NULL,
    [Coordinates] nvarchar(50) NULL,
    [Description] nvarchar(600) NOT NULL,
    [Id] int NOT NULL,
    [Name] nvarchar(80) NOT NULL,
    CONSTRAINT [UQ_NatureMonuments_Key] UNIQUE ([Id])
);
