namespace FormulaTelemetry.Web.Models;

/// <summary>
/// Sector / lap-time colour vs overall best and personal best (motorsport timing).
/// Priority when multiple apply: <see cref="Purple"/> &gt; <see cref="Green"/> &gt; <see cref="Yellow"/>.
/// </summary>
public enum TimingColor
{
    /// <summary>No valid time (null, pit-out, deleted).</summary>
    None = 0,

    /// <summary>Slower than the driver's personal best so far in this session.</summary>
    Yellow = 1,

    /// <summary>Driver's personal best in this sector/lap up to and including this lap.</summary>
    Green = 2,

    /// <summary>Absolute fastest in this sector/lap across all drivers in the session.</summary>
    Purple = 3
}
