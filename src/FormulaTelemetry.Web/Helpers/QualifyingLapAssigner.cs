using FormulaTelemetry.Web.Models;

namespace FormulaTelemetry.Web.Helpers;

/// <summary>Assigns OpenF1 laps to Q1/Q2/Q3 using race-control phase start times.</summary>
public static class QualifyingLapAssigner
{
    public static int? PhaseFor(DateTimeOffset? lapStart, IReadOnlyList<QualifyingPhaseDto> phases)
    {
        if (lapStart is null || phases.Count == 0) return null;

        int? phase = null;
        foreach (var p in phases.OrderBy(x => x.Start))
        {
            if (lapStart.Value >= p.Start)
            {
                phase = p.Phase;
            }
        }

        return phase;
    }

    public static void ApplyPhases(IList<LapTimePointDto> points, IReadOnlyList<LapDto> laps, IReadOnlyList<QualifyingPhaseDto> phases)
    {
        if (phases.Count == 0) return;

        var byKey = laps.ToDictionary(l => (l.DriverNumber, l.LapNumber));
        foreach (var p in points)
        {
            if (!byKey.TryGetValue((p.DriverNumber, p.LapNumber), out var lap)) continue;
            p.QualifyingPhase = PhaseFor(lap.DateStart, phases);
        }
    }

    public static void ApplyPhases(IList<EvaluatedLap> laps, IReadOnlyList<QualifyingPhaseDto> phases)
    {
        if (phases.Count == 0) return;
        foreach (var lap in laps)
        {
            lap.QualifyingPhase = PhaseFor(lap.DateStart, phases);
        }
    }
}
