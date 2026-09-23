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
