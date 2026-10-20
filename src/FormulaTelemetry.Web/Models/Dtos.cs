namespace FormulaTelemetry.Web.Models;

public sealed class MeetingDto
{
    public int MeetingKey { get; set; }
    public string MeetingName { get; set; } = "";
    public string? MeetingOfficialName { get; set; }
    public int Year { get; set; }
    public DateTimeOffset? DateStart { get; set; }
    public DateTimeOffset? DateEnd { get; set; }
    public string? CircuitShortName { get; set; }
    public string? CountryName { get; set; }
    public string? Location { get; set; }
    public bool IsCancelled { get; set; }
}

public sealed class SessionDto
{
    public int SessionKey { get; set; }
    public int MeetingKey { get; set; }
    public string SessionName { get; set; } = "";
    public string SessionType { get; set; } = "";
    public int Year { get; set; }
    public DateTimeOffset? DateStart { get; set; }
    public DateTimeOffset? DateEnd { get; set; }
    public string? CircuitShortName { get; set; }
    public string? Location { get; set; }
    public bool IsCancelled { get; set; }
}

public sealed class SessionResultDto
{
    public int SessionKey { get; set; }
    public int DriverNumber { get; set; }
    public int MeetingKey { get; set; }
    public int? Position { get; set; }
    public int? NumberOfLaps { get; set; }
    public bool Dnf { get; set; }
    public bool Dns { get; set; }
    public bool Dsq { get; set; }
    public string? DurationJson { get; set; }
    public string? GapToLeaderJson { get; set; }
    public string? DriverName { get; set; }
    public string? TeamName { get; set; }
    public string? NameAcronym { get; set; }
}

public sealed class LapDto
{
    public int SessionKey { get; set; }
    public int DriverNumber { get; set; }
    public int LapNumber { get; set; }
    public int MeetingKey { get; set; }
    public DateTimeOffset? DateStart { get; set; }
    public double? LapDuration { get; set; }
    public double? DurationSector1 { get; set; }
    public double? DurationSector2 { get; set; }
    public double? DurationSector3 { get; set; }
    public int? I1Speed { get; set; }
    public int? I2Speed { get; set; }
    public int? StSpeed { get; set; }
    public bool? IsPitOutLap { get; set; }
    public string? DriverName { get; set; }
    public string? TeamName { get; set; }
    public string? NameAcronym { get; set; }
}

/// <summary>One lap sample for the Race Pace chart (full lap + sector splits).</summary>
public sealed class LapTimePointDto
{
    public int LapNumber { get; set; }
    public double LapTimeSeconds { get; set; }
    public double Sector1Seconds { get; set; }
    public double Sector2Seconds { get; set; }
    public double Sector3Seconds { get; set; }
    /// <summary>Speed trap (ST) in km/h when available.</summary>
    public double TopSpeedKmh { get; set; }
    public string? DriverCode { get; set; }
    public string? DriverName { get; set; }
    public int DriverNumber { get; set; }
    public string? Compound { get; set; }
    public int? TyreAge { get; set; }
    public int? StintNumber { get; set; }
    public bool IsPitLap { get; set; }
    /// <summary>1/2/3 for Q1/Q2/Q3 when session is qualifying; otherwise null.</summary>
    public int? QualifyingPhase { get; set; }
    /// <summary>Running race/session position at end of this lap (P1 = 1), when available.</summary>
    public int? Position { get; set; }
    /// <summary>UTC start of this lap (OpenF1 date_start), when available.</summary>
    public DateTimeOffset? DateStart { get; set; }
}

/// <summary>OpenF1 tyre stint for a driver in a session.</summary>
public sealed class StintDto
{
    public int SessionKey { get; set; }
    public int DriverNumber { get; set; }
    public int StintNumber { get; set; }
    public int MeetingKey { get; set; }
    public int? LapStart { get; set; }
    public int? LapEnd { get; set; }
    public string? Compound { get; set; }
    public int? TyreAgeAtStart { get; set; }
}

/// <summary>Start of a qualifying segment from race control (phase 1–3).</summary>
public sealed class QualifyingPhaseDto
{
    public int Phase { get; set; }
    public DateTimeOffset Start { get; set; }
}

public sealed class StartingGridEntryDto
{
    public int SessionKey { get; set; }
    public int DriverNumber { get; set; }
    public int MeetingKey { get; set; }
    public int Position { get; set; }
    public double? LapDuration { get; set; }
}

public sealed class LapPositionDto
{
    public int DriverNumber { get; set; }
    public int LapNumber { get; set; }
    public int Position { get; set; }
}
