namespace FormulaTelemetry.Web.Helpers;

/// <summary>
/// Approximate team hues for UI stripes (data hint). Not official trademarks or logos.
/// </summary>
public static class TeamColors
{
    public static string ForTeam(string? teamName)
    {
        if (string.IsNullOrWhiteSpace(teamName))
        {
            return "#71717A";
        }

        var t = teamName.Trim().ToUpperInvariant();

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

        return "#A1A1AA";
    }

    private static bool Contains(string haystack, string needle) =>
        haystack.Contains(needle, StringComparison.Ordinal);
}
