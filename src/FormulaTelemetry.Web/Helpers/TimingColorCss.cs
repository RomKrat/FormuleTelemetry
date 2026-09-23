using FormulaTelemetry.Web.Models;

namespace FormulaTelemetry.Web.Helpers;

/// <summary>Maps <see cref="TimingColor"/> to CSS classes for Blazor timing tables.</summary>
public static class TimingColorCss
{
    public static string Class(TimingColor color) => color switch
    {
        TimingColor.Purple => "timing-purple",
        TimingColor.Green => "timing-green",
        TimingColor.Yellow => "timing-yellow",
        _ => string.Empty
    };
}
