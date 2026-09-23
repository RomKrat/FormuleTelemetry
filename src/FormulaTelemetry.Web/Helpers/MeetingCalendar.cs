using FormulaTelemetry.Web.Models;

namespace FormulaTelemetry.Web.Helpers;

public static class MeetingCalendar
{
    public static bool IsCompleted(MeetingDto m, DateTime? nowUtcDate = null)
    {
        if (m.IsCancelled || m.DateStart is null) return false;
        var now = nowUtcDate ?? DateTime.UtcNow.Date;
        var end = (m.DateEnd ?? m.DateStart).Value.UtcDateTime.Date;
        return end < now;
    }

    public static (string Label, string Css) Status(MeetingDto m, MeetingDto? nextWeekend = null)
    {
        if (m.IsCancelled) return ("Cancelled", "ft-badge-dns");
        if (m.DateStart is null) return ("TBC", "ft-badge-dns");
        if (IsCompleted(m)) return ("Completed", "ft-badge-done");
        if (nextWeekend is not null && m.MeetingKey == nextWeekend.MeetingKey)
        {
            return ("Next", "ft-badge-next");
        }

        return ("Upcoming", "ft-badge-upcoming");
    }

    public static string DropdownLabel(MeetingDto m, int round)
    {
        var name = (m.MeetingName ?? "").Trim();
        if (name.EndsWith("Grand Prix", StringComparison.OrdinalIgnoreCase))
        {
            name = name[..^"Grand Prix".Length].Trim() + " GP";
        }
        else if (!name.EndsWith(" GP", StringComparison.OrdinalIgnoreCase)
                 && !string.IsNullOrWhiteSpace(m.CircuitShortName))
        {
            name = $"{m.CircuitShortName} GP";
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            name = m.CountryName ?? "Race";
        }

        return $"R{round:00} - {name}";
    }

    public static string ShortLabel(MeetingDto m, int round) => DropdownLabel(m, round);
}
