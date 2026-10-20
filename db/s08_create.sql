IF OBJECT_ID(N'dbo.BikeReservations') IS NULL
CREATE TABLE dbo.[BikeReservations] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_BikeReservations] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [ReservedAt] datetime2(0) NOT NULL,
    [Rented] bit NOT NULL,
    [RentedBikes] int NOT NULL,
    [ReservedBikes] int NOT NULL,
    [StationNumber] int NULL,
    [StationName] nvarchar(30) NOT NULL,
    [Channel] nvarchar(30) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [ExpiresAt] datetime2(0) NOT NULL,
    [RowHash] binary(32) NOT NULL,
    CONSTRAINT [UQ_BikeReservations_Key] UNIQUE ([RowHash])
);
