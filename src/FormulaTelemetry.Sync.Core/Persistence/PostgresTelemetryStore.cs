using FormulaTelemetry.Sync.Core.Configuration;
using FormulaTelemetry.Sync.Core.OpenF1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;

namespace FormulaTelemetry.Sync.Core.Persistence;

public interface ITelemetryStore
{
    Task UpsertMeetingAsync(OpenF1Meeting meeting, CancellationToken cancellationToken = default);
    Task UpsertSessionAsync(OpenF1Session session, CancellationToken cancellationToken = default);
    Task UpsertDriversAsync(IEnumerable<OpenF1Driver> drivers, CancellationToken cancellationToken = default);
    Task UpsertLapsAsync(IEnumerable<OpenF1Lap> laps, CancellationToken cancellationToken = default);
    Task UpsertSessionResultsAsync(IEnumerable<OpenF1SessionResult> results, CancellationToken cancellationToken = default);
    Task UpsertStartingGridAsync(IEnumerable<OpenF1StartingGrid> entries, CancellationToken cancellationToken = default);
    Task UpsertStintsAsync(IEnumerable<OpenF1Stint> stints, CancellationToken cancellationToken = default);
    Task UpsertPitsAsync(IEnumerable<OpenF1Pit> pits, CancellationToken cancellationToken = default);
    Task UpsertIntervalsAsync(IEnumerable<OpenF1Interval> intervals, CancellationToken cancellationToken = default);
    Task UpsertPositionsAsync(IEnumerable<OpenF1Position> positions, CancellationToken cancellationToken = default);
    Task UpsertWeatherAsync(IEnumerable<OpenF1Weather> samples, CancellationToken cancellationToken = default);
    Task UpsertRaceControlAsync(IEnumerable<OpenF1RaceControl> messages, CancellationToken cancellationToken = default);
    Task UpsertOvertakesAsync(IEnumerable<OpenF1Overtake> overtakes, CancellationToken cancellationToken = default);
    Task UpsertTeamRadiosAsync(IEnumerable<OpenF1TeamRadio> radios, CancellationToken cancellationToken = default);
    Task UpsertChampionshipDriversAsync(IEnumerable<OpenF1ChampionshipDriver> rows, CancellationToken cancellationToken = default);
    Task UpsertChampionshipTeamsAsync(IEnumerable<OpenF1ChampionshipTeam> rows, CancellationToken cancellationToken = default);
    Task MarkSyncRunningAsync(int sessionKey, CancellationToken cancellationToken = default);
    Task MarkSyncCompletedAsync(int sessionKey, int lapsCount, CancellationToken cancellationToken = default);
    Task MarkSyncFailedAsync(int sessionKey, string error, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetPendingSessionKeysAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlySet<int>> GetCompletedSessionKeysAsync(CancellationToken cancellationToken = default);
}

public sealed partial class PostgresTelemetryStore : ITelemetryStore
{
    private readonly string _connectionString;
    private readonly ILogger<PostgresTelemetryStore> _logger;

    public PostgresTelemetryStore(IOptions<DatabaseOptions> options, ILogger<PostgresTelemetryStore> logger)
    {
        _connectionString = options.Value.ConnectionString
            ?? throw new InvalidOperationException("Database:ConnectionString is not configured.");
        _logger = logger;
    }

    public async Task UpsertMeetingAsync(OpenF1Meeting m, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO meetings (
                meeting_key, meeting_name, meeting_official_name, date_start, date_end, year,
                circuit_key, circuit_short_name, circuit_type, circuit_image, circuit_info_url,
                country_key, country_code, country_name, country_flag, location, gmt_offset, is_cancelled)
            VALUES (
                @meeting_key, @meeting_name, @meeting_official_name, @date_start, @date_end, @year,
                @circuit_key, @circuit_short_name, @circuit_type, @circuit_image, @circuit_info_url,
                @country_key, @country_code, @country_name, @country_flag, @location, @gmt_offset, @is_cancelled)
            ON CONFLICT (meeting_key) DO UPDATE SET
                meeting_name = EXCLUDED.meeting_name,
                meeting_official_name = EXCLUDED.meeting_official_name,
                date_start = EXCLUDED.date_start,
                date_end = EXCLUDED.date_end,
                year = EXCLUDED.year,
                circuit_key = EXCLUDED.circuit_key,
                circuit_short_name = EXCLUDED.circuit_short_name,
                circuit_type = EXCLUDED.circuit_type,
                circuit_image = EXCLUDED.circuit_image,
                circuit_info_url = EXCLUDED.circuit_info_url,
                country_key = EXCLUDED.country_key,
                country_code = EXCLUDED.country_code,
                country_name = EXCLUDED.country_name,
                country_flag = EXCLUDED.country_flag,
                location = EXCLUDED.location,
                gmt_offset = EXCLUDED.gmt_offset,
                is_cancelled = EXCLUDED.is_cancelled;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("meeting_key", m.MeetingKey);
        cmd.Parameters.AddWithValue("meeting_name", (object?)m.MeetingName ?? "Unknown");
        cmd.Parameters.AddWithValue("meeting_official_name", (object?)m.MeetingOfficialName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("date_start", (object?)m.DateStart ?? DBNull.Value);
        cmd.Parameters.AddWithValue("date_end", (object?)m.DateEnd ?? DBNull.Value);
        cmd.Parameters.AddWithValue("year", m.Year);
        cmd.Parameters.AddWithValue("circuit_key", (object?)m.CircuitKey ?? DBNull.Value);
        cmd.Parameters.AddWithValue("circuit_short_name", (object?)m.CircuitShortName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("circuit_type", (object?)m.CircuitType ?? DBNull.Value);
        cmd.Parameters.AddWithValue("circuit_image", (object?)m.CircuitImage ?? DBNull.Value);
        cmd.Parameters.AddWithValue("circuit_info_url", (object?)m.CircuitInfoUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("country_key", (object?)m.CountryKey ?? DBNull.Value);
        cmd.Parameters.AddWithValue("country_code", (object?)m.CountryCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("country_name", (object?)m.CountryName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("country_flag", (object?)m.CountryFlag ?? DBNull.Value);
        cmd.Parameters.AddWithValue("location", (object?)m.Location ?? DBNull.Value);
        cmd.Parameters.AddWithValue("gmt_offset", (object?)m.GmtOffset ?? DBNull.Value);
        cmd.Parameters.AddWithValue("is_cancelled", m.IsCancelled ?? false);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpsertSessionAsync(OpenF1Session s, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO sessions (
                session_key, meeting_key, session_name, session_type, date_start, date_end, year,
                circuit_key, circuit_short_name, country_key, country_code, country_name,
                location, gmt_offset, is_cancelled)
            VALUES (
                @session_key, @meeting_key, @session_name, @session_type, @date_start, @date_end, @year,
                @circuit_key, @circuit_short_name, @country_key, @country_code, @country_name,
                @location, @gmt_offset, @is_cancelled)
            ON CONFLICT (session_key) DO UPDATE SET
                meeting_key = EXCLUDED.meeting_key,
                session_name = EXCLUDED.session_name,
                session_type = EXCLUDED.session_type,
                date_start = EXCLUDED.date_start,
                date_end = EXCLUDED.date_end,
                year = EXCLUDED.year,
                circuit_key = EXCLUDED.circuit_key,
                circuit_short_name = EXCLUDED.circuit_short_name,
                country_key = EXCLUDED.country_key,
                country_code = EXCLUDED.country_code,
                country_name = EXCLUDED.country_name,
                location = EXCLUDED.location,
                gmt_offset = EXCLUDED.gmt_offset,
                is_cancelled = EXCLUDED.is_cancelled;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("session_key", s.SessionKey);
        cmd.Parameters.AddWithValue("meeting_key", s.MeetingKey);
        cmd.Parameters.AddWithValue("session_name", (object?)s.SessionName ?? "Unknown");
        cmd.Parameters.AddWithValue("session_type", (object?)s.SessionType ?? "Unknown");
        cmd.Parameters.AddWithValue("date_start", (object?)s.DateStart ?? DBNull.Value);
        cmd.Parameters.AddWithValue("date_end", (object?)s.DateEnd ?? DBNull.Value);
        cmd.Parameters.AddWithValue("year", s.Year);
        cmd.Parameters.AddWithValue("circuit_key", (object?)s.CircuitKey ?? DBNull.Value);
        cmd.Parameters.AddWithValue("circuit_short_name", (object?)s.CircuitShortName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("country_key", (object?)s.CountryKey ?? DBNull.Value);
        cmd.Parameters.AddWithValue("country_code", (object?)s.CountryCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("country_name", (object?)s.CountryName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("location", (object?)s.Location ?? DBNull.Value);
        cmd.Parameters.AddWithValue("gmt_offset", (object?)s.GmtOffset ?? DBNull.Value);
        cmd.Parameters.AddWithValue("is_cancelled", s.IsCancelled ?? false);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpsertDriversAsync(IEnumerable<OpenF1Driver> drivers, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO session_drivers (
                session_key, driver_number, meeting_key, broadcast_name, full_name, first_name, last_name,
                name_acronym, team_name, team_colour, headshot_url, country_code)
            VALUES (
                @session_key, @driver_number, @meeting_key, @broadcast_name, @full_name, @first_name, @last_name,
                @name_acronym, @team_name, @team_colour, @headshot_url, @country_code)
            ON CONFLICT (session_key, driver_number) DO UPDATE SET
                meeting_key = EXCLUDED.meeting_key,
                broadcast_name = EXCLUDED.broadcast_name,
                full_name = EXCLUDED.full_name,
                first_name = EXCLUDED.first_name,
                last_name = EXCLUDED.last_name,
                name_acronym = EXCLUDED.name_acronym,
                team_name = EXCLUDED.team_name,
                team_colour = EXCLUDED.team_colour,
                headshot_url = EXCLUDED.headshot_url,
                country_code = EXCLUDED.country_code;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        foreach (var d in drivers)
        {
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("session_key", d.SessionKey);
            cmd.Parameters.AddWithValue("driver_number", d.DriverNumber);
            cmd.Parameters.AddWithValue("meeting_key", d.MeetingKey);
            cmd.Parameters.AddWithValue("broadcast_name", (object?)d.BroadcastName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("full_name", (object?)d.FullName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("first_name", (object?)d.FirstName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("last_name", (object?)d.LastName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("name_acronym", (object?)d.NameAcronym ?? DBNull.Value);
            cmd.Parameters.AddWithValue("team_name", (object?)d.TeamName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("team_colour", (object?)d.TeamColour ?? DBNull.Value);
            cmd.Parameters.AddWithValue("headshot_url", (object?)d.HeadshotUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("country_code", (object?)d.CountryCode ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task UpsertLapsAsync(IEnumerable<OpenF1Lap> laps, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO laps (
                session_key, driver_number, lap_number, meeting_key, date_start, lap_duration,
                duration_sector_1, duration_sector_2, duration_sector_3,
                i1_speed, i2_speed, st_speed, is_pit_out_lap,
                segments_sector_1, segments_sector_2, segments_sector_3)
            VALUES (
                @session_key, @driver_number, @lap_number, @meeting_key, @date_start, @lap_duration,
                @duration_sector_1, @duration_sector_2, @duration_sector_3,
                @i1_speed, @i2_speed, @st_speed, @is_pit_out_lap,
                @segments_sector_1, @segments_sector_2, @segments_sector_3)
            ON CONFLICT (session_key, driver_number, lap_number) DO UPDATE SET
                meeting_key = EXCLUDED.meeting_key,
                date_start = EXCLUDED.date_start,
                lap_duration = EXCLUDED.lap_duration,
                duration_sector_1 = EXCLUDED.duration_sector_1,
                duration_sector_2 = EXCLUDED.duration_sector_2,
                duration_sector_3 = EXCLUDED.duration_sector_3,
                i1_speed = EXCLUDED.i1_speed,
                i2_speed = EXCLUDED.i2_speed,
                st_speed = EXCLUDED.st_speed,
                is_pit_out_lap = EXCLUDED.is_pit_out_lap,
                segments_sector_1 = EXCLUDED.segments_sector_1,
                segments_sector_2 = EXCLUDED.segments_sector_2,
                segments_sector_3 = EXCLUDED.segments_sector_3;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        foreach (var lap in laps)
        {
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("session_key", lap.SessionKey);
            cmd.Parameters.AddWithValue("driver_number", lap.DriverNumber);
            cmd.Parameters.AddWithValue("lap_number", lap.LapNumber);
            cmd.Parameters.AddWithValue("meeting_key", lap.MeetingKey);
            cmd.Parameters.AddWithValue("date_start", (object?)lap.DateStart ?? DBNull.Value);
            cmd.Parameters.AddWithValue("lap_duration", (object?)lap.LapDuration ?? DBNull.Value);
            cmd.Parameters.AddWithValue("duration_sector_1", (object?)lap.DurationSector1 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("duration_sector_2", (object?)lap.DurationSector2 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("duration_sector_3", (object?)lap.DurationSector3 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("i1_speed", (object?)lap.I1Speed ?? DBNull.Value);
            cmd.Parameters.AddWithValue("i2_speed", (object?)lap.I2Speed ?? DBNull.Value);
            cmd.Parameters.AddWithValue("st_speed", (object?)lap.StSpeed ?? DBNull.Value);
            cmd.Parameters.AddWithValue("is_pit_out_lap", (object?)lap.IsPitOutLap ?? DBNull.Value);

            var s1 = cmd.Parameters.Add("segments_sector_1", NpgsqlDbType.Array | NpgsqlDbType.Integer);
            s1.Value = (object?)ToIntArray(lap.SegmentsSector1) ?? DBNull.Value;
            var s2 = cmd.Parameters.Add("segments_sector_2", NpgsqlDbType.Array | NpgsqlDbType.Integer);
            s2.Value = (object?)ToIntArray(lap.SegmentsSector2) ?? DBNull.Value;
            var s3 = cmd.Parameters.Add("segments_sector_3", NpgsqlDbType.Array | NpgsqlDbType.Integer);
            s3.Value = (object?)ToIntArray(lap.SegmentsSector3) ?? DBNull.Value;

            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task MarkSyncRunningAsync(int sessionKey, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO sync_jobs (session_key, status, started_at, updated_at)
            VALUES (@session_key, 'Running', now(), now())
            ON CONFLICT (session_key) DO UPDATE SET
                status = 'Running',
                started_at = now(),
                error_message = NULL,
                updated_at = now();
            """;

        await using var conn = await OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("session_key", sessionKey);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task MarkSyncCompletedAsync(int sessionKey, int lapsCount, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE sync_jobs
            SET status = 'Completed',
                completed_at = now(),
                laps_synced = true,
                error_message = NULL,
                updated_at = now()
            WHERE session_key = @session_key;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("session_key", sessionKey);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogInformation("Session {SessionKey} sync completed ({Laps} laps).", sessionKey, lapsCount);
    }

    public async Task MarkSyncFailedAsync(int sessionKey, string error, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO sync_jobs (session_key, status, error_message, updated_at)
            VALUES (@session_key, 'Failed', @error, now())
            ON CONFLICT (session_key) DO UPDATE SET
                status = 'Failed',
                error_message = EXCLUDED.error_message,
                updated_at = now();
            """;

        await using var conn = await OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("session_key", sessionKey);
        cmd.Parameters.AddWithValue("error", error);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetPendingSessionKeysAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT s.session_key
            FROM sessions s
            LEFT JOIN sync_jobs j ON j.session_key = s.session_key
            WHERE s.date_end IS NOT NULL
              AND s.date_end < now()
              AND s.is_cancelled = false
              AND (j.session_key IS NULL OR j.status IN ('Pending', 'Failed'))
            ORDER BY s.date_end;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        var keys = new List<int>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            keys.Add(reader.GetInt32(0));
        }

        return keys;
    }

    public async Task<IReadOnlySet<int>> GetCompletedSessionKeysAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT session_key
            FROM sync_jobs
            WHERE status = 'Completed';
            """;

        await using var conn = await OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        var keys = new HashSet<int>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            keys.Add(reader.GetInt32(0));
        }

        return keys;
    }

    private async Task<NpgsqlConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        return conn;
    }

    /// <summary>OpenF1 may send nulls inside segment arrays; store 0 for null entries.</summary>
    private static int[]? ToIntArray(int?[]? values)
    {
        if (values is null)
        {
            return null;
        }

        var result = new int[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            result[i] = values[i] ?? 0;
        }

        return result;
    }
}
