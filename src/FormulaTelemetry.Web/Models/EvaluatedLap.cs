namespace FormulaTelemetry.Web.Models;

/// <summary>One timed split (lap or sector) with evaluated colour.</summary>
public sealed class TimedSplit
{
    public double? Seconds { get; init; }
    public TimingColor Color { get; init; }

    public static TimedSplit Empty { get; } = new() { Seconds = null, Color = TimingColor.None };
}

/// <summary>
/// <see cref="LapDto"/> plus Overall Best / Personal Best / No Improvement colours
/// for lap time and each sector.
/// </summary>
public sealed class EvaluatedLap
{
    public required LapDto Source { get; init; }

    public TimedSplit LapTime { get; init; } = TimedSplit.Empty;
    public TimedSplit Sector1 { get; init; } = TimedSplit.Empty;
    public TimedSplit Sector2 { get; init; } = TimedSplit.Empty;
    public TimedSplit Sector3 { get; init; } = TimedSplit.Empty;

    /// <summary>1/2/3 for Q1/Q2/Q3 when session is qualifying; otherwise null.</summary>
    public int? QualifyingPhase { get; set; }

    public int SessionKey => Source.SessionKey;
    public int DriverNumber => Source.DriverNumber;
    public int LapNumber => Source.LapNumber;
    public DateTimeOffset? DateStart => Source.DateStart;
    public int? StSpeed => Source.StSpeed;
    public bool? IsPitOutLap => Source.IsPitOutLap;
    public string? DriverName => Source.DriverName;
    public string? TeamName => Source.TeamName;
    public string? NameAcronym => Source.NameAcronym;

    public string DisplayName =>
        DriverName ?? NameAcronym ?? DriverNumber.ToString();
}
