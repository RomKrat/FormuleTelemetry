using FormulaTelemetry.Web.Models;

namespace FormulaTelemetry.Web.Helpers;

/// <summary>
/// Orders drivers for the shared selection bar by session classification.
/// Race → finish position (DNF/DNS/DSQ last); Quali/Practice → best / segment time.
/// </summary>
public static class DriverOrder
{
    public static bool IsRaceSession(string? sessionName, string? sessionType = null)
    {
        var n = (sessionName ?? "").ToLowerInvariant();
        if (n.Contains("qualifying") || n.Contains("shootout") || n == "q") return false;
        if (n.Contains("practice") || n.Contains("fp1") || n.Contains("fp2") || n.Contains("fp3")) return false;
        if (n.Contains("race")) return true;
        var t = (sessionType ?? "").ToLowerInvariant();
        return t.Contains("race");
    }

    /// <summary>
    /// Returns driver numbers sorted for the selection bar.
    /// <paramref name="focusSegmentIndex"/> is 0-based Q1/Q2/Q3 when filtering a quali segment; null = overall.
    /// </summary>
    public static List<int> OrderDriverNumbers(
        IEnumerable<int> driverNumbers,
        IReadOnlyList<SessionResultDto>? results,
        string? sessionName,
        string? sessionType = null,
        int? focusSegmentIndex = null,
        IReadOnlyDictionary<int, int>? startingGrid = null)
    {
        var drivers = driverNumbers.Distinct().ToList();
        if (drivers.Count <= 1) return drivers;

        var byDriver = (results ?? [])
            .GroupBy(r => r.DriverNumber)
            .ToDictionary(g => g.Key, g => g.First());

        var race = IsRaceSession(sessionName, sessionType);

        return drivers
            .OrderBy(d => SortKey(d, byDriver, race, focusSegmentIndex, startingGrid))
            .ToList();
    }

    public static IOrderedEnumerable<T> OrderByClassification<T>(
        IEnumerable<T> items,
        Func<T, int> driverNumber,
        IReadOnlyList<SessionResultDto>? results,
        string? sessionName,
        string? sessionType = null,
        int? focusSegmentIndex = null,
        IReadOnlyDictionary<int, int>? startingGrid = null)
    {
        var race = IsRaceSession(sessionName, sessionType);
        var byDriver = (results ?? [])
            .GroupBy(r => r.DriverNumber)
            .ToDictionary(g => g.Key, g => g.First());

        return items.OrderBy(item =>
            SortKey(driverNumber(item), byDriver, race, focusSegmentIndex, startingGrid));
    }

    private static (int Bucket, double Key, int Tie) SortKey(
        int driverNumber,
        IReadOnlyDictionary<int, SessionResultDto> byDriver,
        bool race,
        int? focusSegmentIndex,
        IReadOnlyDictionary<int, int>? startingGrid)
    {
        byDriver.TryGetValue(driverNumber, out var result);

        if (race)
        {
            if (result is not null)
            {
                if (result.Dsq) return (3, 0, driverNumber);
                if (result.Dns) return (2, 0, driverNumber);
                if (result.Dnf)
                {
                    // Fewer laps → further back among retirements.
                    var laps = result.NumberOfLaps ?? 0;
                    return (1, -laps, driverNumber);
                }

                if (result.Position is int pos)
                {
                    return (0, pos, driverNumber);
                }
            }

            return FallbackKey(driverNumber, startingGrid);
        }

        // Qualifying / Practice / Shootout — by segment or final classification time.
        if (result is not null)
        {
            double? time = focusSegmentIndex is int seg
                ? ResultDisplay.SegmentAt(result.DurationJson, seg)
                : ResultDisplay.FinalSegmentTime(result.DurationJson);

            if (time is > 0)
            {
                return (0, time.Value, driverNumber);
            }

            // Reached classification position without a parseable time (rare).
            if (focusSegmentIndex is null && result.Position is int pos)
            {
                return (0, pos, driverNumber);
            }

            // Did not set a time in this segment / session.
            return (1, result.Position ?? 999, driverNumber);
        }

        return FallbackKey(driverNumber, startingGrid);
    }

    private static (int Bucket, double Key, int Tie) FallbackKey(
        int driverNumber,
        IReadOnlyDictionary<int, int>? startingGrid)
    {
        if (startingGrid is not null && startingGrid.TryGetValue(driverNumber, out var gridPos))
        {
            return (8, gridPos, driverNumber);
        }

        return (9, driverNumber, driverNumber);
    }
}
