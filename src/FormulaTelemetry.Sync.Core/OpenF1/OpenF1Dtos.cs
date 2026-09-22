using System.Text.Json.Serialization;

namespace FormulaTelemetry.Sync.Core.OpenF1;

public sealed class OpenF1Meeting
{
    [JsonPropertyName("meeting_key")]
    public int MeetingKey { get; set; }

    [JsonPropertyName("meeting_name")]
    public string? MeetingName { get; set; }

    [JsonPropertyName("meeting_official_name")]
    public string? MeetingOfficialName { get; set; }

    [JsonPropertyName("date_start")]
    public DateTimeOffset? DateStart { get; set; }

    [JsonPropertyName("date_end")]
    public DateTimeOffset? DateEnd { get; set; }

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("circuit_key")]
    public int? CircuitKey { get; set; }

    [JsonPropertyName("circuit_short_name")]
    public string? CircuitShortName { get; set; }

    [JsonPropertyName("circuit_type")]
    public string? CircuitType { get; set; }

    [JsonPropertyName("circuit_image")]
    public string? CircuitImage { get; set; }

    [JsonPropertyName("circuit_info_url")]
    public string? CircuitInfoUrl { get; set; }

    [JsonPropertyName("country_key")]
    public int? CountryKey { get; set; }

    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("country_name")]
    public string? CountryName { get; set; }

    [JsonPropertyName("country_flag")]
    public string? CountryFlag { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("gmt_offset")]
    public string? GmtOffset { get; set; }

    [JsonPropertyName("is_cancelled")]
    public bool? IsCancelled { get; set; }
}

public sealed class OpenF1Session
{
    [JsonPropertyName("session_key")]
    public int SessionKey { get; set; }

    [JsonPropertyName("meeting_key")]
    public int MeetingKey { get; set; }

    [JsonPropertyName("session_name")]
    public string? SessionName { get; set; }

    [JsonPropertyName("session_type")]
    public string? SessionType { get; set; }

    [JsonPropertyName("date_start")]
    public DateTimeOffset? DateStart { get; set; }

    [JsonPropertyName("date_end")]
    public DateTimeOffset? DateEnd { get; set; }

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("circuit_key")]
    public int? CircuitKey { get; set; }

    [JsonPropertyName("circuit_short_name")]
    public string? CircuitShortName { get; set; }

    [JsonPropertyName("country_key")]
    public int? CountryKey { get; set; }

    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("country_name")]
    public string? CountryName { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("gmt_offset")]
    public string? GmtOffset { get; set; }

    [JsonPropertyName("is_cancelled")]
    public bool? IsCancelled { get; set; }
}

public sealed class OpenF1Driver
{
    [JsonPropertyName("session_key")]
    public int SessionKey { get; set; }

    [JsonPropertyName("meeting_key")]
    public int MeetingKey { get; set; }

    [JsonPropertyName("driver_number")]
    public int DriverNumber { get; set; }

    [JsonPropertyName("broadcast_name")]
    public string? BroadcastName { get; set; }

    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("name_acronym")]
    public string? NameAcronym { get; set; }

    [JsonPropertyName("team_name")]
    public string? TeamName { get; set; }

    [JsonPropertyName("team_colour")]
    public string? TeamColour { get; set; }

    [JsonPropertyName("headshot_url")]
    public string? HeadshotUrl { get; set; }

    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }
}

public sealed class OpenF1Lap
{
    [JsonPropertyName("session_key")]
    public int SessionKey { get; set; }

    [JsonPropertyName("meeting_key")]
    public int MeetingKey { get; set; }

    [JsonPropertyName("driver_number")]
    public int DriverNumber { get; set; }

    [JsonPropertyName("lap_number")]
    public int LapNumber { get; set; }

    [JsonPropertyName("date_start")]
    public DateTimeOffset? DateStart { get; set; }

    [JsonPropertyName("lap_duration")]
    public double? LapDuration { get; set; }

    [JsonPropertyName("duration_sector_1")]
    public double? DurationSector1 { get; set; }

    [JsonPropertyName("duration_sector_2")]
    public double? DurationSector2 { get; set; }

    [JsonPropertyName("duration_sector_3")]
    public double? DurationSector3 { get; set; }

    [JsonPropertyName("i1_speed")]
    public int? I1Speed { get; set; }

    [JsonPropertyName("i2_speed")]
    public int? I2Speed { get; set; }

    [JsonPropertyName("st_speed")]
    public int? StSpeed { get; set; }

    [JsonPropertyName("is_pit_out_lap")]
    public bool? IsPitOutLap { get; set; }

    [JsonPropertyName("segments_sector_1")]
    public int?[]? SegmentsSector1 { get; set; }

    [JsonPropertyName("segments_sector_2")]
    public int?[]? SegmentsSector2 { get; set; }

    [JsonPropertyName("segments_sector_3")]
    public int?[]? SegmentsSector3 { get; set; }
}
