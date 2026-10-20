IF OBJECT_ID(N'dbo.SchoolSportsClubs') IS NULL
CREATE TABLE dbo.[SchoolSportsClubs] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_SchoolSportsClubs] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [Id] nvarchar(10) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [RegisteredOn] date NULL,
    [Street] nvarchar(40) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    CONSTRAINT [UQ_SchoolSportsClubs_Key] UNIQUE ([Id])
);
