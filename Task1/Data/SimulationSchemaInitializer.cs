using Assignment_4.Models;
using Microsoft.EntityFrameworkCore;

namespace Assignment_4.Data;

public static class SimulationSchemaInitializer
{
    public static async Task EnsureTablesAsync(Dat154Gr2Context db)
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        await using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SimulationSessions'";
        var exists = await checkCmd.ExecuteScalarAsync();
        if (exists != null)
            return;

        string patientTable;
        await using (var tcmd = connection.CreateCommand())
        {
            tcmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ('Patient','Patients') ORDER BY CASE TABLE_NAME WHEN 'Patient' THEN 0 ELSE 1 END";
            await using var r = await tcmd.ExecuteReaderAsync();
            if (!await r.ReadAsync())
                return;
            patientTable = r.GetString(0);
        }

        var sql = $@"
CREATE TABLE [SimulationSessions] (
    [Id] int NOT NULL IDENTITY,
    [PatientId] int NOT NULL,
    [StudentCustomUserId] int NOT NULL,
    [StartedAt] datetimeoffset NOT NULL,
    [EndedAt] datetimeoffset NULL,
    CONSTRAINT [PK_SimulationSessions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SimulationSessions_Patient] FOREIGN KEY ([PatientId]) REFERENCES [{patientTable}]([Id]),
    CONSTRAINT [FK_SimulationSessions_CustomUsers] FOREIGN KEY ([StudentCustomUserId]) REFERENCES [CustomUsers]([Id])
);
CREATE TABLE [SimulationActions] (
    [Id] int NOT NULL IDENTITY,
    [SessionId] int NOT NULL,
    [OccurredAt] datetimeoffset NOT NULL,
    [Kind] nvarchar(max) NOT NULL,
    [Drug] nvarchar(max) NULL,
    [DoseMg] float NULL,
    [Route] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    CONSTRAINT [PK_SimulationActions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SimulationActions_SimulationSessions] FOREIGN KEY ([SessionId]) REFERENCES [SimulationSessions]([Id]) ON DELETE CASCADE
);
CREATE TABLE [TeacherObservations] (
    [Id] int NOT NULL IDENTITY,
    [SessionId] int NOT NULL,
    [RelatedActionId] int NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [Text] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_TeacherObservations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TeacherObservations_SimulationSessions] FOREIGN KEY ([SessionId]) REFERENCES [SimulationSessions]([Id]) ON DELETE CASCADE
);
CREATE TABLE [SimulationDeviations] (
    [Id] int NOT NULL IDENTITY,
    [ActionId] int NOT NULL,
    [RuleName] nvarchar(max) NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_SimulationDeviations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SimulationDeviations_SimulationActions] FOREIGN KEY ([ActionId]) REFERENCES [SimulationActions]([Id]) ON DELETE CASCADE
);";

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
