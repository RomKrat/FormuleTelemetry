using System.Globalization;
using System.Text.Json;
using FormulaTelemetry.Web.Models;

namespace FormulaTelemetry.Web.Helpers;

public static class UiFormat
{
    private static readonly string[] TeamPalette =
    [
        "#E8A317", "#2BBF6A", "#5B8DEF", "#F07178", "#C084FC",
        "#22D3EE", "#F0B429", "#A3E635", "#FB7185", "#94A3B8"
    ];

    public static string SessionTabLabel(string sessionName, string sessionType)
    {
        var n = (sessionName ?? "").ToLowerInvariant();
        if (n.Contains("sprint") && (n.Contains("qual") || n.Contains("shootout"))) return "SQ";
        if (n.Contains("sprint")) return "SPRINT";
        if (n.Contains("race")) return "RACE";
        if (n.Contains("qualifying") || n == "q") return "QUALI";
        if (n.Contains("practice 1") || n.Contains("fp1")) return "FP1";
        if (n.Contains("practice 2") || n.Contains("fp2")) return "FP2";
        if (n.Contains("practice 3") || n.Contains("fp3")) return "FP3";
        return SessionBadge(sessionName, sessionType).ToUpperInvariant();
    }

    public static string SessionBadge(string sessionName, string sessionType)
    {
        var n = (sessionName ?? "").ToLowerInvariant();
        if (n.Contains("race") && !n.Contains("sprint")) return "Race";
        if (n.Contains("sprint")) return "Sprint";
        if (n.Contains("qualifying") || n == "q") return "Q";
        if (n.Contains("sprint shootout") || n.Contains("sprint qualifying")) return "SQ";
        if (n.Contains("practice 1") || n.Contains("fp1")) return "FP1";
        if (n.Contains("practice 2") || n.Contains("fp2")) return "FP2";
        if (n.Contains("practice 3") || n.Contains("fp3")) return "FP3";
        return string.IsNullOrWhiteSpace(sessionType) ? "Session" : sessionType;
    }

    public static string MeetingStatus(MeetingDto m)
    {
        var now = DateTimeOffset.UtcNow;
        if (m.IsCancelled) return "CANCELLED";
        if (m.DateEnd is { } end && end < now) return "COMPLETED";
        if (m.DateStart is { } start && start <= now && (m.DateEnd is null || m.DateEnd >= now))
            return "LIVE";
        return "UPCOMING";
    }

    public static string StatusBadgeClass(string status) => status switch
    {
        "COMPLETED" => "badge badge-done",
        "UPCOMING" or "LIVE" => "badge badge-next",
        _ => "badge"
    };

    public static string TeamAccent(string? teamName)
    {
        if (string.IsNullOrWhiteSpace(teamName)) return TeamPalette[0];
        var hash = teamName.Aggregate(0, (h, c) => unchecked(h * 31 + c));
        return TeamPalette[Math.Abs(hash) % TeamPalette.Length];
    }

    public static string FormatGap(string? gapJson)
    {
        if (string.IsNullOrWhiteSpace(gapJson) || gapJson is "null") return "—";
        try
        {
            using var doc = JsonDocument.Parse(gapJson);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Number && root.TryGetDouble(out var d))
                return FormatSignedSeconds(d);
            if (root.ValueKind == JsonValueKind.String)
                return root.GetString() ?? "—";
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                var last = root[root.GetArrayLength() - 1];
                if (last.ValueKind == JsonValueKind.Number && last.TryGetDouble(out var ad))
                    return FormatSignedSeconds(ad);
                if (last.ValueKind == JsonValueKind.String)
                    return last.GetString() ?? "—";
            }
        }
        catch
        {
            // fall through
        }

        return gapJson.Trim('"');
    }

    public static string FormatMeetingDateRange(DateTimeOffset? dateStart, DateTimeOffset? dateEnd)
    {
        if (dateStart is null) return "—";

        var start = dateStart.Value.UtcDateTime.Date;
        if (dateEnd is null)
        {
            return start.ToString("dd MMM yyyy");
        }

        var end = dateEnd.Value.UtcDateTime.Date;
        if (end <= start)
        {
            return start.ToString("dd MMM yyyy");
        }

        if (start.Year == end.Year && start.Month == end.Month)
        {
            return $"{start:dd}–{end:dd MMM yyyy}";
        }

        if (start.Year == end.Year)
        {
            return $"{start:dd MMM} – {end:dd MMM yyyy}";
        }

        return $"{start:dd MMM yyyy} – {end:dd MMM yyyy}";
    }

    public static string FormatLapTime(double? seconds) =>
        ResultDisplay.FormatLapSeconds(seconds);

    public static string FormatSector(double? seconds) =>
        seconds is null or <= 0 ? "—" : seconds.Value.ToString("0.000", CultureInfo.InvariantCulture);

    private static string FormatSignedSeconds(double d)
    {
        if (Math.Abs(d) < 0.0005) return "0.000";
        var sign = d > 0 ? "+" : "";
        return sign + d.ToString("0.000", CultureInfo.InvariantCulture);
    }

    public static string PosRowClass(int? position) => position switch
    {
        1 => "pos-p1",
        2 => "pos-p2",
        3 => "pos-p3",
        _ => ""
    };
}
