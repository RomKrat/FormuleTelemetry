using System.Text.Json.Serialization;

namespace FormulaTelemetry.Web.Helpers;

/// <summary>
/// Real F1 circuit outlines (GPS-derived, normalized to a 0–100 viewBox).
/// Data loaded from wwwroot/data/track-layouts.json (bacinger/f1-circuits GeoJSON).
/// </summary>
public static class TrackLayouts
{
    public sealed record TurnMark(double Progress, string Label);

    public sealed record Layout(
        string Name,
        IReadOnlyList<(double X, double Y)> Points,
        IReadOnlyList<TurnMark> Turns,
        double Sector1End = 0.33,
        double Sector2End = 0.67);

    private static readonly object Gate = new();
    private static List<(string[] Aliases, Layout Layout)> _catalog = [];
    private static bool _loaded;

    public static bool IsLoaded
    {
        get { lock (Gate) return _loaded; }
    }

    public static void LoadFromDto(TrackLayoutFileDto? file)
    {
        if (file?.Circuits is null || file.Circuits.Count == 0) return;

        var list = new List<(string[] Aliases, Layout Layout)>();
        foreach (var c in file.Circuits)
        {
            if (c.Points is null || c.Points.Count < 3) continue;
            var pts = c.Points
                .Select(p => p is { Count: >= 2 } ? (p[0], p[1]) : (50.0, 50.0))
                .ToList();
            var turns = (c.Turns ?? [])
                .Select(t => new TurnMark(t.Progress, t.Label ?? ""))
                .Where(t => !string.IsNullOrWhiteSpace(t.Label))
                .ToList();
            var aliases = (c.Aliases ?? [])
                .Select(Normalize)
                .Where(a => a.Length > 0)
                .Distinct()
                .ToArray();
            if (aliases.Length == 0)
            {
                aliases = [Normalize(c.Location), Normalize(c.Name)];
            }

            list.Add((aliases, new Layout(
                c.Name ?? c.Location ?? "Circuit",
                pts,
                turns,
                c.Sector1End is > 0 and < 1 ? c.Sector1End : 0.33,
                c.Sector2End is > 0 and < 1 ? c.Sector2End : 0.67)));
        }

        lock (Gate)
        {
            _catalog = list;
            _loaded = list.Count > 0;
        }
    }

    public static Layout Resolve(string? circuitShortName, string? location = null)
    {
        lock (Gate)
        {
            foreach (var key in CandidateKeys(circuitShortName, location))
            {
                foreach (var (aliases, layout) in _catalog)
                {
                    if (aliases.Any(a =>
                            key.Equals(a, StringComparison.Ordinal)
                            || key.Contains(a, StringComparison.Ordinal)
                            || a.Contains(key, StringComparison.Ordinal)))
                    {
                        return layout;
                    }
                }
            }
        }

        return Fallback;
    }

    private static IEnumerable<string> CandidateKeys(string? circuitShortName, string? location)
    {
        foreach (var raw in new[] { circuitShortName, location })
        {
            var key = Normalize(raw);
            if (key.Length == 0) continue;
            yield return key;

            // OpenF1 sometimes uses "Spa-Francorchamps" / "Circuit de …"
            var spaced = key.Replace('-', ' ');
            if (spaced != key) yield return spaced;
        }
    }

    public static (double X, double Y) PointAt(Layout layout, double progress01)
    {
        var pts = layout.Points;
        if (pts.Count == 0) return (50, 50);
        if (pts.Count == 1) return pts[0];

        var lengths = new double[pts.Count];
        var total = 0.0;
        for (var i = 1; i < pts.Count; i++)
        {
            var dx = pts[i].X - pts[i - 1].X;
            var dy = pts[i].Y - pts[i - 1].Y;
            total += Math.Sqrt(dx * dx + dy * dy);
            lengths[i] = total;
        }

        if (total < 1e-6) return pts[0];

        var target = Math.Clamp(progress01, 0, 1) * total;
        for (var i = 1; i < pts.Count; i++)
        {
            if (lengths[i] >= target)
            {
                var seg = lengths[i] - lengths[i - 1];
                var u = seg < 1e-9 ? 0 : (target - lengths[i - 1]) / seg;
                return (
                    pts[i - 1].X + (pts[i].X - pts[i - 1].X) * u,
                    pts[i - 1].Y + (pts[i].Y - pts[i - 1].Y) * u);
            }
        }

        return pts[^1];
    }

    public static IReadOnlyList<(double X, double Y)> SectorSlice(Layout layout, int sector /*1..3*/)
    {
        var a = sector switch
        {
            1 => 0,
            2 => layout.Sector1End,
            _ => layout.Sector2End
        };
        var b = sector switch
        {
            1 => layout.Sector1End,
            2 => layout.Sector2End,
            _ => 1.0
        };

        // denser sampling for GPS-quality paths
        const int samples = 96;
        var list = new List<(double X, double Y)>(samples + 1);
        for (var i = 0; i <= samples; i++)
        {
            var p = a + (b - a) * i / samples;
            list.Add(PointAt(layout, p));
        }

        return list;
    }

    private static string Normalize(string? s) =>
        (s ?? "").Trim().ToUpperInvariant()
            .Replace("É", "E", StringComparison.Ordinal)
            .Replace("Á", "A", StringComparison.Ordinal)
            .Replace("Ã", "A", StringComparison.Ordinal)
            .Replace("Ó", "O", StringComparison.Ordinal)
            .Replace("Ú", "U", StringComparison.Ordinal)
            .Replace("Ü", "U", StringComparison.Ordinal)
            .Replace("Ñ", "N", StringComparison.Ordinal);

    private static readonly Layout Fallback = new(
        "Circuit",
        [
            (50, 88), (72, 82), (88, 62), (90, 40), (78, 18), (55, 10),
            (32, 14), (14, 32), (10, 55), (18, 76), (36, 88), (50, 88)
        ],
        [
            new TurnMark(0.08, "1"), new TurnMark(0.28, "4"), new TurnMark(0.48, "7"),
            new TurnMark(0.68, "10"), new TurnMark(0.88, "14")
        ],
        0.33, 0.67);
}

public sealed class TrackLayoutFileDto
{
    [JsonPropertyName("circuits")]
    public List<TrackLayoutCircuitDto>? Circuits { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }
}

public sealed class TrackLayoutCircuitDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("aliases")]
    public List<string>? Aliases { get; set; }

    [JsonPropertyName("points")]
    public List<List<double>>? Points { get; set; }

    [JsonPropertyName("turns")]
    public List<TrackLayoutTurnDto>? Turns { get; set; }

    [JsonPropertyName("sector1End")]
    public double Sector1End { get; set; } = 0.33;

    [JsonPropertyName("sector2End")]
    public double Sector2End { get; set; } = 0.67;
}

public sealed class TrackLayoutTurnDto
{
    [JsonPropertyName("progress")]
    public double Progress { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }
}
