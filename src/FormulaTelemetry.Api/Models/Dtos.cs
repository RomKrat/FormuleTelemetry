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

public sealed record StintDto(
    int SessionKey,
    int DriverNumber,
    int StintNumber,
    int MeetingKey,
    int? LapStart,
    int? LapEnd,
    string? Compound,
    int? TyreAgeAtStart);

/// <summary>Start timestamp of a qualifying segment (Q1/Q2/Q3) from race control.</summary>
public sealed record QualifyingPhaseDto(
    int Phase,
    DateTimeOffset Start);

public sealed record StartingGridEntryDto(
    int SessionKey,
    int DriverNumber,
    int MeetingKey,
    int Position,
    double? LapDuration);

/// <summary>Race/session position for a driver at the end of a given lap.</summary>
public sealed record LapPositionDto(
    int DriverNumber,
    int LapNumber,
    int Position);
