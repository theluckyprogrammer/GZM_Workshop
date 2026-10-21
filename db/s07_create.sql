IF OBJECT_ID(N'dbo.SportsFacilities') IS NULL
CREATE TABLE dbo.[SportsFacilities] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_SportsFacilities] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [Facility] nvarchar(100) NOT NULL,
    [AddressManager] nvarchar(150) NOT NULL,
    [ObjectId] int NOT NULL,
    CONSTRAINT [UQ_SportsFacilities_Key] UNIQUE ([ObjectId])
);
