using System.Globalization;
using System.Text.Json;
using FormulaTelemetry.Web.Models;

namespace FormulaTelemetry.Web.Helpers;

public static class ResultDisplay
{
    public static bool IsSegmentedResults(IReadOnlyList<SessionResultDto> results) =>
        MaxSegmentCount(results) >= 2;

    public static int MaxSegmentCount(IReadOnlyList<SessionResultDto> results)
    {
        var max = 0;
        foreach (var r in results)
        {
            var n = ParseSecondsParts(r.DurationJson).Count;
            if (n > max) max = n;
        }

        return max;
    }

    /// <summary>Q1 / Q2 / Q3 (or fewer) segment times in seconds; null = no time / did not reach.</summary>
    public static IReadOnlyList<double?> ParseSecondsParts(string? durationJson)
    {
        if (string.IsNullOrWhiteSpace(durationJson) || durationJson == "null")
        {
            return Array.Empty<double?>();
        }

        try
        {
            using var doc = JsonDocument.Parse(durationJson);
            return ParseParts(doc.RootElement);
        }
        catch (JsonException)
        {
            if (TryParseDouble(durationJson.Trim('"'), out var one))
            {
                return new double?[] { one };
            }

            return Array.Empty<double?>();
        }
    }

    public static string FormatDuration(string? durationJson)
    {
        var parts = ParseSecondsParts(durationJson);
        if (parts.Count == 0) return "—";
        if (parts.Count == 1) return FormatLapSeconds(parts[0]);

        return string.Join(" / ", parts.Select(FormatLapSeconds));
    }

    public static string FormatLapSeconds(double? seconds)
    {
        if (seconds is null || double.IsNaN(seconds.Value) || double.IsInfinity(seconds.Value) || seconds <= 0)
        {
            return "—";
        }

        var total = seconds.Value;
        var hours = (int)(total / 3600);
        var minutes = (int)(total / 60) % 60;
        var secs = total - (hours * 3600) - (minutes * 60);

        if (hours > 0)
        {
            return $"{hours}:{minutes.ToString("00", CultureInfo.InvariantCulture)}:{secs.ToString("00.000", CultureInfo.InvariantCulture)}";
        }

        if (minutes > 0)
        {
            return $"{minutes}:{secs.ToString("00.000", CultureInfo.InvariantCulture)}";
        }

        return secs.ToString("0.000", CultureInfo.InvariantCulture);
    }

    public static string FormatGapDelta(double? seconds)
    {
        if (seconds is null || double.IsNaN(seconds.Value) || double.IsInfinity(seconds.Value))
        {
            return "—";
        }

        var s = seconds.Value;
        if (Math.Abs(s) < 0.0005) return "0.000";
        var sign = s > 0 ? "+" : string.Empty;
        return sign + s.ToString("0.000", CultureInfo.InvariantCulture);
    }

    public static string FormatGap(string? gapJson)
    {
        if (string.IsNullOrWhiteSpace(gapJson) || gapJson == "null")
        {
            return "—";
        }

        try
        {
            using var doc = JsonDocument.Parse(gapJson);
            var parts = ParseParts(doc.RootElement);
            if (parts.Count == 0) return "—";
            // Prefer final segment gap when API sends an array.
            for (var i = parts.Count - 1; i >= 0; i--)
            {
                if (parts[i] is not null)
                {
                    return FormatGapDelta(parts[i]);
                }
            }

            return "—";
        }
        catch (JsonException)
        {
            return gapJson.Trim('"');
        }
    }

    /// <summary>Last recorded segment time (Q3 if reached, else Q2/Q1).</summary>
    public static double? FinalSegmentTime(string? durationJson)
    {
        var parts = ParseSecondsParts(durationJson);
        for (var i = parts.Count - 1; i >= 0; i--)
        {
            if (parts[i] is > 0)
            {
                return parts[i];
            }
        }

        return null;
    }

    public static double? SegmentAt(string? durationJson, int index)
    {
        var parts = ParseSecondsParts(durationJson);
        return index >= 0 && index < parts.Count ? parts[index] : null;
    }

    public static string PodiumClass(int? position) => position switch
    {
        1 => "podium-1",
        2 => "podium-2",
        3 => "podium-3",
        _ => string.Empty
    };

    public static string SegmentHeader(int index, int total, string? sessionName)
    {
        var n = (sessionName ?? "").ToLowerInvariant();
        var sprint = n.Contains("sprint") && (n.Contains("qual") || n.Contains("shootout"));
        var prefix = sprint ? "SQ" : "Q";
        return $"{prefix}{index + 1}";
    }

    public static string SegmentPrefix(string? sessionName)
    {
        var n = (sessionName ?? "").ToLowerInvariant();
        return n.Contains("sprint") && (n.Contains("qual") || n.Contains("shootout")) ? "SQ" : "Q";
    }

    /// <summary>
    /// Classic F1 cutoffs: last position that progressed to Q3 / Q2.
    /// Returns (q3Cutoff, q2Cutoff) e.g. (10, 15) → dividers after those rows.
    /// </summary>
    public static (int Q3Cutoff, int Q2Cutoff)? QualifyingCutoffs(
        IReadOnlyList<SessionResultDto> results,
        int segments)
    {
        if (segments < 3 || results.Count == 0) return null;

        var reachedQ3 = results.Count(r => SegmentAt(r.DurationJson, 2) is > 0);
        var reachedQ2 = results.Count(r => SegmentAt(r.DurationJson, 1) is > 0);

        if (reachedQ3 is >= 8 and <= 12 && reachedQ2 > reachedQ3)
        {
            return (reachedQ3, reachedQ2);
        }

        // Standard 20-car weekend fallback.
        if (results.Count >= 16)
        {
            return (10, 15);
        }

        if (reachedQ3 >= 1 && reachedQ2 > reachedQ3)
        {
            return (reachedQ3, reachedQ2);
        }

        return null;
    }

    /// <summary>Status chip for segmented quali (DNF/DNS/DSQ take priority).</summary>
    public static (string Label, string Css)? QualifyingStatus(
        SessionResultDto r,
        int segments,
        string? sessionName,
        (int Q3Cutoff, int Q2Cutoff)? cutoffs)
    {
        if (r.Dsq) return ("DSQ", "ft-badge ft-badge-dsq");
        if (r.Dnf) return ("DNF", "ft-badge ft-badge-dnf");
        if (r.Dns) return ("DNS", "ft-badge ft-badge-dns");
        if (segments < 3 || cutoffs is null || r.Position is not int pos) return null;

        var prefix = SegmentPrefix(sessionName);
        var (q3, q2) = cutoffs.Value;

        if (pos <= q3) return ($"{prefix}3", "ft-badge ft-badge-q3");
        if (pos <= q2) return ($"{prefix}2 Eliminated", "ft-badge ft-badge-q2-out");
        return ($"{prefix}1 Eliminated", "ft-badge ft-badge-q1-out");
    }

    public static bool ReachedSegment(string? durationJson, int segmentIndex) =>
        SegmentAt(durationJson, segmentIndex) is > 0;

    private static List<double?> ParseParts(JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Number:
                return el.TryGetDouble(out var d) ? [d] : [];
            case JsonValueKind.String:
            {
                var s = el.GetString()?.Trim();
                if (string.IsNullOrEmpty(s) || s is "null") return [null];
                return TryParseDouble(s, out var n) ? [n] : [null];
            }
            case JsonValueKind.Array:
            {
                var list = new List<double?>();
                foreach (var item in el.EnumerateArray())
                {
                    if (item.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                    {
                        list.Add(null);
                        continue;
                    }

                    if (item.ValueKind == JsonValueKind.Number && item.TryGetDouble(out var ad))
                    {
                        list.Add(ad > 0 ? ad : null);
                        continue;
                    }

                    if (item.ValueKind == JsonValueKind.String)
                    {
                        var s = item.GetString()?.Trim();
                        if (string.IsNullOrEmpty(s) || s is "null")
                        {
                            list.Add(null);
                        }
                        else if (TryParseDouble(s, out var sn) && sn > 0)
                        {
                            list.Add(sn);
                        }
                        else
                        {
                            list.Add(null);
                        }

                        continue;
                    }

                    list.Add(null);
                }

                return list;
            }
            default:
                return [];
        }
    }

    private static bool TryParseDouble(string s, out double value) =>
        double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}
