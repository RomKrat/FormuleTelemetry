namespace FormulaTelemetry.Api.Models;

public sealed record MeetingDto(
    int MeetingKey,
    string MeetingName,
    string? MeetingOfficialName,
    int Year,
    DateTimeOffset? DateStart,
    DateTimeOffset? DateEnd,
    string? CircuitShortName,
    string? CountryName,
    string? Location,
    bool IsCancelled);

public sealed record SessionDto(
    int SessionKey,
    int MeetingKey,
    string SessionName,
    string SessionType,
    int Year,
    DateTimeOffset? DateStart,
    DateTimeOffset? DateEnd,
    string? CircuitShortName,
    string? Location,
    bool IsCancelled);

public sealed record SessionResultDto(
    int SessionKey,
    int DriverNumber,
    int MeetingKey,
    int? Position,
    int? NumberOfLaps,
    bool Dnf,
    bool Dns,
    bool Dsq,
    string? DurationJson,
    string? GapToLeaderJson,
    string? DriverName,
    string? TeamName,
    string? NameAcronym);

public sealed record LapDto(
    int SessionKey,
    int DriverNumber,
    int LapNumber,
    int MeetingKey,
    DateTimeOffset? DateStart,
    double? LapDuration,
    double? DurationSector1,
    double? DurationSector2,
    double? DurationSector3,
    int? I1Speed,
    int? I2Speed,
    int? StSpeed,
    bool? IsPitOutLap,
    string? DriverName,
    string? TeamName,
    string? NameAcronym);
