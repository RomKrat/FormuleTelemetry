using System.Text.Json;
using System.Text.Json.Serialization;

namespace FormulaTelemetry.Sync.Core.OpenF1;

public sealed class OpenF1SessionResult
{
    [JsonPropertyName("session_key")]
    public int SessionKey { get; set; }

    [JsonPropertyName("meeting_key")]
    public int MeetingKey { get; set; }

    [JsonPropertyName("driver_number")]
    public int DriverNumber { get; set; }

    [JsonPropertyName("position")]
    public int? Position { get; set; }

    [JsonPropertyName("duration")]
    public JsonElement? Duration { get; set; }

    [JsonPropertyName("gap_to_leader")]
    public JsonElement? GapToLeader { get; set; }

    [JsonPropertyName("number_of_laps")]
    public int? NumberOfLaps { get; set; }

    [JsonPropertyName("dnf")]
    public bool? Dnf { get; set; }

    [JsonPropertyName("dns")]
    public bool? Dns { get; set; }

    [JsonPropertyName("dsq")]
    public bool? Dsq { get; set; }
}

public sealed class OpenF1StartingGrid
{
    [JsonPropertyName("session_key")]
    public int SessionKey { get; set; }

    [JsonPropertyName("meeting_key")]
    public int MeetingKey { get; set; }

    [JsonPropertyName("driver_number")]
    public int DriverNumber { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("lap_duration")]
    public double? LapDuration { get; set; }
}

public sealed class OpenF1Stint
{
    [JsonPropertyName("session_key")]
    public int SessionKey { get; set; }

    [JsonPropertyName("meeting_key")]
    public int MeetingKey { get; set; }

    [JsonPropertyName("driver_number")]
    public int DriverNumber { get; set; }

    [JsonPropertyName("stint_number")]
    public int StintNumber { get; set; }

    [JsonPropertyName("lap_start")]
    public int? LapStart { get; set; }

    [JsonPropertyName("lap_end")]
    public int? LapEnd { get; set; }

    [JsonPropertyName("compound")]
    public string? Compound { get; set; }

    [JsonPropertyName("tyre_age_at_start")]
    public int? TyreAgeAtStart { get; set; }
}

public sealed class OpenF1Pit
{
    [JsonPropertyName("session_key")]
    public int SessionKey { get; set; }

    [JsonPropertyName("meeting_key")]
    public int MeetingKey { get; set; }

    [JsonPropertyName("driver_number")]
    public int DriverNumber { get; set; }

    [JsonPropertyName("lap_number")]
    public int LapNumber { get; set; }

    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; set; }

    [JsonPropertyName("lane_duration")]
    public double? LaneDuration { get; set; }

    [JsonPropertyName("stop_duration")]
    public double? StopDuration { get; set; }
}

public sealed class OpenF1Interval
{
    [JsonPropertyName("session_key")]
    public int SessionKey { get; set; }

    [JsonPropertyName("meeting_key")]
    public int MeetingKey { get; set; }

    [JsonPropertyName("driver_number")]
    public int DriverNumber { get; set; }

    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; set; }

    [JsonPropertyName("gap_to_leader")]
    public JsonElement? GapToLeader { get; set; }

    [JsonPropertyName("interval")]
    public JsonElement? Interval { get; set; }
}

public sealed class OpenF1Position
{
    [JsonPropertyName("session_key")]
    public int SessionKey { get; set; }

    [JsonPropertyName("meeting_key")]
    public int MeetingKey { get; set; }

    [JsonPropertyName("driver_number")]
    public int DriverNumber { get; set; }

    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }
}

public sealed class OpenF1Weather
{
    [JsonPropertyName("session_key")]
    public int SessionKey { get; set; }

    [JsonPropertyName("meeting_key")]
    public int MeetingKey { get; set; }

    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; set; }

    [JsonPropertyName("air_temperature")]
    public double? AirTemperature { get; set; }

    [JsonPropertyName("track_temperature")]
    public double? TrackTemperature { get; set; }

    [JsonPropertyName("humidity")]
    public double? Humidity { get; set; }

    [JsonPropertyName("pressure")]
    public double? Pressure { get; set; }

    [JsonPropertyName("rainfall")]
    public JsonElement? Rainfall { get; set; }

    [JsonPropertyName("wind_direction")]
    public int? WindDirection { get; set; }

    [JsonPropertyName("wind_speed")]
    public double? WindSpeed { get; set; }
}

public sealed class OpenF1RaceControl
{
    [JsonPropertyName("session_key")]
    public int SessionKey { get; set; }

    [JsonPropertyName("meeting_key")]
    public int MeetingKey { get; set; }

    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("driver_number")]
    public int? DriverNumber { get; set; }

    [JsonPropertyName("flag")]
    public string? Flag { get; set; }

    [JsonPropertyName("lap_number")]
    public int? LapNumber { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("qualifying_phase")]
    public int? QualifyingPhase { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    [JsonPropertyName("sector")]
    public int? Sector { get; set; }
}

public sealed class OpenF1Overtake
{
    [JsonPropertyName("session_key")]
    public int SessionKey { get; set; }

    [JsonPropertyName("meeting_key")]
    public int MeetingKey { get; set; }

    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; set; }

    [JsonPropertyName("overtaking_driver_number")]
    public int OvertakingDriverNumber { get; set; }

    [JsonPropertyName("overtaken_driver_number")]
    public int OvertakenDriverNumber { get; set; }

    [JsonPropertyName("position")]
    public int? Position { get; set; }
}

public sealed class OpenF1TeamRadio
{
    [JsonPropertyName("session_key")]
    public int SessionKey { get; set; }

    [JsonPropertyName("meeting_key")]
    public int MeetingKey { get; set; }

    [JsonPropertyName("driver_number")]
    public int DriverNumber { get; set; }

    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; set; }

    [JsonPropertyName("recording_url")]
    public string? RecordingUrl { get; set; }
}

public sealed class OpenF1ChampionshipDriver
{
    [JsonPropertyName("session_key")]
    public int SessionKey { get; set; }

    [JsonPropertyName("meeting_key")]
    public int MeetingKey { get; set; }

    [JsonPropertyName("driver_number")]
    public int DriverNumber { get; set; }

    [JsonPropertyName("points_start")]
    public double? PointsStart { get; set; }

    [JsonPropertyName("points_current")]
    public double? PointsCurrent { get; set; }

    [JsonPropertyName("position_start")]
    public int? PositionStart { get; set; }

    [JsonPropertyName("position_current")]
    public int? PositionCurrent { get; set; }
}

public sealed class OpenF1ChampionshipTeam
{
    [JsonPropertyName("session_key")]
    public int SessionKey { get; set; }

    [JsonPropertyName("meeting_key")]
    public int MeetingKey { get; set; }

    [JsonPropertyName("team_name")]
    public string TeamName { get; set; } = string.Empty;

    [JsonPropertyName("points_start")]
    public double? PointsStart { get; set; }

    [JsonPropertyName("points_current")]
    public double? PointsCurrent { get; set; }

    [JsonPropertyName("position_start")]
    public int? PositionStart { get; set; }

    [JsonPropertyName("position_current")]
    public int? PositionCurrent { get; set; }
}

internal static class JsonElementHelpers
{
    public static string? ToJsonb(JsonElement? element)
    {
        if (element is null || element.Value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return null;
        }

        return element.Value.GetRawText();
    }

    public static string? ToFlexibleText(JsonElement? element)
    {
        if (element is null || element.Value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return null;
        }

        return element.Value.ValueKind switch
        {
            JsonValueKind.String => element.Value.GetString(),
            JsonValueKind.Number => element.Value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => element.Value.GetRawText()
        };
    }

    public static bool? ToFlexibleBool(JsonElement? element)
    {
        if (element is null || element.Value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return null;
        }

        return element.Value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => element.Value.TryGetInt32(out var n) && n != 0,
            JsonValueKind.String when bool.TryParse(element.Value.GetString(), out var b) => b,
            JsonValueKind.String when int.TryParse(element.Value.GetString(), out var n) => n != 0,
            _ => null
        };
    }
}
