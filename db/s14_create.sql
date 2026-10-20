IF OBJECT_ID(N'dbo.TicketMachines') IS NULL
CREATE TABLE dbo.[TicketMachines] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_TicketMachines] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [No] int NOT NULL,
    [Commune] nvarchar(40) NOT NULL,
    [MachineCode] nvarchar(20) NOT NULL,
    [StopNumber] int NULL,
    [StopName] nvarchar(80) NOT NULL,
    [LocationDescription] nvarchar(150) NULL,
    [Platform] nvarchar(10) NULL,
    [Coordinates] nvarchar(60) NULL,
    CONSTRAINT [UQ_TicketMachines_Key] UNIQUE ([MachineCode])
);
