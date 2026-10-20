namespace FormulaTelemetry.Web.Helpers;

/// <summary>
/// Approximate team hues for UI stripes (data hint). Not official trademarks or logos.
/// </summary>
public static class TeamColors
{
    public static string ForTeam(string? teamName) => ForTeam(teamName, teammateIndex: 0);

    /// <param name="teammateIndex">0 = primary team colour; 1+ = distinct shade for the other car.</param>
    public static string ForTeam(string? teamName, int teammateIndex)
    {
        if (string.IsNullOrWhiteSpace(teamName))
        {
            return teammateIndex == 0 ? "#71717A" : "#A1A1AA";
        }

        var t = teamName.Trim().ToUpperInvariant();
        var primary = PrimaryForTeam(t);
        if (teammateIndex <= 0) return primary;

        // Second / third car: shift toward a clearly different but related tone.
        return teammateIndex switch
        {
            1 => AltForTeam(t, primary),
            _ => Mix(primary, "#FFFFFF", 0.35)
        };
    }

    private static string PrimaryForTeam(string t)
    {
        if (Contains(t, "FERRARI")) return "#E8002D";
        if (Contains(t, "MERCEDES")) return "#27F4D2";
        if (Contains(t, "MCLAREN")) return "#FF8000";
        if (Contains(t, "RED BULL") || t is "RBR") return "#3671C6";
        if (Contains(t, "ASTON")) return "#229971";
        if (Contains(t, "ALPINE")) return "#FF87BC";
        if (Contains(t, "WILLIAMS")) return "#64C4FF";
        if (Contains(t, "HAAS")) return "#B6BABD";
        if (Contains(t, "SAUBER") || Contains(t, "STAKE") || Contains(t, "KICK") || Contains(t, "ALFA")) return "#52E252";
        if (Contains(t, "RACING BULLS") || Contains(t, "RB ") || t.EndsWith(" RB") || t == "RB"
            || Contains(t, "ALPHATAURI") || Contains(t, "VISA")) return "#6692FF";
        if (Contains(t, "CADILLAC") || Contains(t, "AUDI")) return "#A1A1AA";

        return "#A1A1AA";
    }

    private static string AltForTeam(string t, string primary)
    {
        if (Contains(t, "MERCEDES")) return "#0B9B84"; // darker teal — readable vs #27F4D2
        if (Contains(t, "FERRARI")) return "#FF6B6B";
        if (Contains(t, "MCLAREN")) return "#FFB347";
        if (Contains(t, "RED BULL") || t is "RBR") return "#7EB0F0";
        if (Contains(t, "ASTON")) return "#5ED4A8";
        if (Contains(t, "ALPINE")) return "#FFB0D6";
        if (Contains(t, "WILLIAMS")) return "#A8DEFF";
        if (Contains(t, "HAAS")) return "#E8E8EA";
        if (Contains(t, "SAUBER") || Contains(t, "STAKE") || Contains(t, "KICK") || Contains(t, "ALFA")) return "#8AFF8A";
        if (Contains(t, "RACING BULLS") || Contains(t, "RB ") || t.EndsWith(" RB") || t == "RB"
            || Contains(t, "ALPHATAURI") || Contains(t, "VISA")) return "#9BB6FF";
        return Mix(primary, "#FFFFFF", 0.28);
    }

    private static string Mix(string hexA, string hexB, double t)
    {
        static (int R, int G, int B) Parse(string hex)
        {
            hex = hex.TrimStart('#');
            return (
                Convert.ToInt32(hex[..2], 16),
                Convert.ToInt32(hex[2..4], 16),
                Convert.ToInt32(hex[4..6], 16));
        }

        var a = Parse(hexA);
        var b = Parse(hexB);
        var r = (int)Math.Round(a.R + (b.R - a.R) * t);
        var g = (int)Math.Round(a.G + (b.G - a.G) * t);
        var bl = (int)Math.Round(a.B + (b.B - a.B) * t);
        return $"#{r:X2}{g:X2}{bl:X2}";
    }

    private static bool Contains(string haystack, string needle) =>
        haystack.Contains(needle, StringComparison.Ordinal);
}
