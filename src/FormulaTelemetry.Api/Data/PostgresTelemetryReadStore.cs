using FormulaTelemetry.Api.Configuration;
using FormulaTelemetry.Api.Models;
using Microsoft.Extensions.Options;
using Npgsql;

namespace FormulaTelemetry.Api.Data;

public sealed class PostgresTelemetryReadStore : ITelemetryReadStore
{
    private readonly string _connectionString;

    public PostgresTelemetryReadStore(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.ConnectionString
            ?? throw new InvalidOperationException("Database:ConnectionString is missing.");
    }

    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        await using var conn = await OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand("SELECT 1", conn);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is not null;
    }

    public async Task<IReadOnlyList<int>> GetAvailableYearsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT DISTINCT year
            FROM meetings
            ORDER BY year DESC
            """;

        await using var conn = await OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        var list = new List<int>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(reader.GetInt32(0));
        }

        return list;
    }

    public async Task<IReadOnlyList<MeetingDto>> GetMeetingsByYearAsync(int year, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT meeting_key, meeting_name, meeting_official_name, year,
                   date_start, date_end, circuit_short_name, country_name, location, is_cancelled
            FROM meetings
            WHERE year = @year
            ORDER BY date_start NULLS LAST, meeting_key
            """;

        await using var conn = await OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("year", year);

        var list = new List<MeetingDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(ReadMeeting(reader));
        }

        return list;
    }

    public async Task<MeetingDto?> GetMeetingAsync(int meetingKey, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT meeting_key, meeting_name, meeting_official_name, year,
                   date_start, date_end, circuit_short_name, country_name, location, is_cancelled
            FROM meetings
            WHERE meeting_key = @meeting_key
            """;

        await using var conn = await OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("meeting_key", meetingKey);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadMeeting(reader);
    }

    public async Task<IReadOnlyList<SessionDto>> GetSessionsByMeetingAsync(int meetingKey, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT session_key, meeting_key, session_name, session_type, year,
                   date_start, date_end, circuit_short_name, location, is_cancelled
            FROM sessions
            WHERE meeting_key = @meeting_key
            ORDER BY date_start NULLS LAST, session_key
            """;

        await using var conn = await OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("meeting_key", meetingKey);

        var list = new List<SessionDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(ReadSession(reader));
        }

        return list;
    }

    public async Task<SessionDto?> GetSessionAsync(int sessionKey, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT session_key, meeting_key, session_name, session_type, year,
                   date_start, date_end, circuit_short_name, location, is_cancelled
            FROM sessions
            WHERE session_key = @session_key
            """;

        await using var conn = await OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("session_key", sessionKey);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadSession(reader);
    }

    public async Task<IReadOnlyList<SessionResultDto>> GetSessionResultsAsync(int sessionKey, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT r.session_key, r.driver_number, r.meeting_key, r.position, r.number_of_laps,
                   r.dnf, r.dns, r.dsq,
                   r.duration_json::text, r.gap_to_leader_json::text,
                   d.full_name, d.team_name, d.name_acronym
            FROM session_results r
            LEFT JOIN session_drivers d
              ON d.session_key = r.session_key AND d.driver_number = r.driver_number
            WHERE r.session_key = @session_key
            ORDER BY r.position NULLS LAST, r.driver_number
            """;

        await using var conn = await OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("session_key", sessionKey);

        var list = new List<SessionResultDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new SessionResultDto(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.IsDBNull(3) ? null : reader.GetInt32(3),
                reader.IsDBNull(4) ? null : reader.GetInt32(4),
                reader.GetBoolean(5),
                reader.GetBoolean(6),
                reader.GetBoolean(7),
                reader.IsDBNull(8) ? null : reader.GetString(8),
                reader.IsDBNull(9) ? null : reader.GetString(9),
                reader.IsDBNull(10) ? null : reader.GetString(10),
                reader.IsDBNull(11) ? null : reader.GetString(11),
                reader.IsDBNull(12) ? null : reader.GetString(12)));
        }

        return list;
    }

    public async Task<IReadOnlyList<LapDto>> GetLapsAsync(int sessionKey, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT l.session_key, l.driver_number, l.lap_number, l.meeting_key, l.date_start,
                   l.lap_duration, l.duration_sector_1, l.duration_sector_2, l.duration_sector_3,
                   l.i1_speed, l.i2_speed, l.st_speed, l.is_pit_out_lap,
                   d.full_name, d.team_name, d.name_acronym
            FROM laps l
            LEFT JOIN session_drivers d
              ON d.session_key = l.session_key AND d.driver_number = l.driver_number
            WHERE l.session_key = @session_key
            ORDER BY l.lap_number, l.driver_number
            """;

        await using var conn = await OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("session_key", sessionKey);

        var list = new List<LapDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new LapDto(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.IsDBNull(4) ? null : reader.GetFieldValue<DateTimeOffset>(4),
                reader.IsDBNull(5) ? null : reader.GetDouble(5),
                reader.IsDBNull(6) ? null : reader.GetDouble(6),
                reader.IsDBNull(7) ? null : reader.GetDouble(7),
                reader.IsDBNull(8) ? null : reader.GetDouble(8),
                reader.IsDBNull(9) ? null : reader.GetInt32(9),
                reader.IsDBNull(10) ? null : reader.GetInt32(10),
                reader.IsDBNull(11) ? null : reader.GetInt32(11),
                reader.IsDBNull(12) ? null : reader.GetBoolean(12),
                reader.IsDBNull(13) ? null : reader.GetString(13),
                reader.IsDBNull(14) ? null : reader.GetString(14),
                reader.IsDBNull(15) ? null : reader.GetString(15)));
        }

        return list;
    }

    private async Task<NpgsqlConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        return conn;
    }

    private static MeetingDto ReadMeeting(NpgsqlDataReader reader) =>
        new(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetString(2),
            reader.GetInt32(3),
            reader.IsDBNull(4) ? null : reader.GetFieldValue<DateTimeOffset>(4),
            reader.IsDBNull(5) ? null : reader.GetFieldValue<DateTimeOffset>(5),
            reader.IsDBNull(6) ? null : reader.GetString(6),
            reader.IsDBNull(7) ? null : reader.GetString(7),
            reader.IsDBNull(8) ? null : reader.GetString(8),
            reader.GetBoolean(9));

    private static SessionDto ReadSession(NpgsqlDataReader reader) =>
        new(
            reader.GetInt32(0),
            reader.GetInt32(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetInt32(4),
            reader.IsDBNull(5) ? null : reader.GetFieldValue<DateTimeOffset>(5),
            reader.IsDBNull(6) ? null : reader.GetFieldValue<DateTimeOffset>(6),
            reader.IsDBNull(7) ? null : reader.GetString(7),
            reader.IsDBNull(8) ? null : reader.GetString(8),
            reader.GetBoolean(9));
}
