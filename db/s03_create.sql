CREATE TABLE IF NOT EXISTS [BikeAvailability] (
    [RecordId] INTEGER PRIMARY KEY AUTOINCREMENT,
    [SourceRow] INTEGER NOT NULL,
    [Day] TEXT NOT NULL,
    [Commune] TEXT NOT NULL,
    [AvailableBikes] INTEGER NOT NULL,
    [RequiredBikes] INTEGER NOT NULL,
    [Exceeded] INTEGER NOT NULL,
    CONSTRAINT [UQ_BikeAvailability_Key] UNIQUE ([Day], [Commune])
);
