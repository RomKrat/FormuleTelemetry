namespace FormulaTelemetry.Sync.Core.Models;

public sealed class SyncRequest
{
    public int? Year { get; init; }
    public int? Round { get; init; }
    public int? MeetingKey { get; init; }
    public int? SessionKey { get; init; }
    public bool PendingOnly { get; init; }
    public bool Latest { get; init; }
    public bool AutoDiscover { get; init; }

    public static SyncRequest ForYear(int year) => new() { Year = year };
    public static SyncRequest ForYearRound(int year, int round) => new() { Year = year, Round = round };
    public static SyncRequest ForMeeting(int meetingKey) => new() { MeetingKey = meetingKey };
    public static SyncRequest ForSession(int sessionKey) => new() { SessionKey = sessionKey };
    public static SyncRequest ForLatest() => new() { Latest = true };
    public static SyncRequest Pending() => new() { PendingOnly = true };
    public static SyncRequest ForAutoDiscover() => new() { AutoDiscover = true };
}

public sealed class SyncResult
{
    public required bool Success { get; init; }
    public int SessionsProcessed { get; init; }
    public int MeetingsUpserted { get; init; }
    public int DriversUpserted { get; init; }
    public int LapsUpserted { get; init; }
    public IReadOnlyList<string> Messages { get; init; } = [];
    public string? Error { get; init; }

    public static SyncResult Fail(string error) => new()
    {
        Success = false,
        Error = error
    };
}
