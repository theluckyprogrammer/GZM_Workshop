IF OBJECT_ID(N'dbo.BikeAvailability') IS NULL
CREATE TABLE dbo.[BikeAvailability] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_BikeAvailability] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [Day] date NOT NULL,
    [Commune] nvarchar(40) NOT NULL,
    [AvailableBikes] int NOT NULL,
    [RequiredBikes] int NOT NULL,
    [Exceeded] bit NOT NULL,
    CONSTRAINT [UQ_BikeAvailability_Key] UNIQUE ([Day], [Commune])
);
