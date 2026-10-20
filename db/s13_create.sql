IF OBJECT_ID(N'dbo.CultureClasses') IS NULL
CREATE TABLE dbo.[CultureClasses] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_CultureClasses] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [ClassType] nvarchar(40) NOT NULL,
    [Day] nvarchar(40) NULL,
    [Id] int NOT NULL,
    [MinAge] decimal(4,1) NULL,
    [Room] nvarchar(60) NULL,
    [Section] nvarchar(80) NOT NULL,
    [StartHour] time(0) NULL,
    [Unit] nvarchar(30) NOT NULL,
    CONSTRAINT [UQ_CultureClasses_Key] UNIQUE ([Id])
);
