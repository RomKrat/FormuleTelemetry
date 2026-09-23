using FormulaTelemetry.Web.Models;

namespace FormulaTelemetry.Web.Helpers;

/// <summary>
/// Evaluates lap/sector colours:
/// Purple = overall session best; Green = personal best so far; Yellow = no improvement.
/// </summary>
public static class LapTimingEvaluator
{
    private const double Epsilon = 0.0005;

    public static IReadOnlyList<EvaluatedLap> Evaluate(IEnumerable<LapDto> laps)
    {
        var list = laps.ToList();
        if (list.Count == 0) return [];

        var overallLap = MinValid(list.Select(l => l.LapDuration));
        var overallS1 = MinValid(list.Select(l => l.DurationSector1));
        var overallS2 = MinValid(list.Select(l => l.DurationSector2));
        var overallS3 = MinValid(list.Select(l => l.DurationSector3));

        // Personal best "so far" requires chronological order within the session.
        var ordered = list
            .OrderBy(l => l.DateStart ?? DateTimeOffset.MaxValue)
            .ThenBy(l => l.LapNumber)
            .ThenBy(l => l.DriverNumber)
            .ToList();

        var pbLap = new Dictionary<int, double>();
        var pbS1 = new Dictionary<int, double>();
        var pbS2 = new Dictionary<int, double>();
        var pbS3 = new Dictionary<int, double>();

        var byKey = new Dictionary<(int Driver, int Lap), EvaluatedLap>();

        foreach (var lap in ordered)
        {
            var evaluated = new EvaluatedLap
            {
                Source = lap,
                LapTime = Colorize(lap.LapDuration, overallLap, pbLap, lap.DriverNumber),
                Sector1 = Colorize(lap.DurationSector1, overallS1, pbS1, lap.DriverNumber),
                Sector2 = Colorize(lap.DurationSector2, overallS2, pbS2, lap.DriverNumber),
                Sector3 = Colorize(lap.DurationSector3, overallS3, pbS3, lap.DriverNumber),
            };
            byKey[(lap.DriverNumber, lap.LapNumber)] = evaluated;
        }

        // Preserve the caller's / API row order for the table.
        return list
            .Select(l => byKey.TryGetValue((l.DriverNumber, l.LapNumber), out var e)
                ? e
                : new EvaluatedLap { Source = l })
            .ToList();
    }

    private static TimedSplit Colorize(
        double? seconds,
        double? overallBest,
        Dictionary<int, double> personalBestByDriver,
        int driverNumber)
    {
        if (!IsValid(seconds))
        {
            return TimedSplit.Empty;
        }

        var value = seconds!.Value;
        var isOverall = overallBest is not null && NearlyEqual(value, overallBest.Value);

        personalBestByDriver.TryGetValue(driverNumber, out var pbSoFar);
        var hasPb = personalBestByDriver.ContainsKey(driverNumber);

        TimingColor color;
        if (isOverall)
        {
            color = TimingColor.Purple;
        }
        else if (!hasPb)
        {
            // First valid split establishes the baseline — no green wash on out-laps.
            color = TimingColor.None;
        }
        else if (value < pbSoFar - Epsilon || NearlyEqual(value, pbSoFar))
        {
            color = TimingColor.Green;
        }
        else
        {
            color = TimingColor.Yellow;
        }

        if (!hasPb || value < pbSoFar - Epsilon)
        {
            personalBestByDriver[driverNumber] = value;
        }

        return new TimedSplit { Seconds = value, Color = color };
    }

    private static double? MinValid(IEnumerable<double?> values)
    {
        double? best = null;
        foreach (var v in values)
        {
            if (!IsValid(v)) continue;
            if (best is null || v!.Value < best.Value) best = v;
        }

        return best;
    }

    private static bool IsValid(double? seconds) =>
        seconds is > 0 && !double.IsNaN(seconds.Value) && !double.IsInfinity(seconds.Value);

    private static bool NearlyEqual(double a, double b) => Math.Abs(a - b) < Epsilon;
}
