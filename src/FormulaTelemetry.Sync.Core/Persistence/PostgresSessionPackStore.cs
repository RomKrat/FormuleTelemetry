using FormulaTelemetry.Sync.Core.OpenF1;
using Npgsql;
using NpgsqlTypes;

namespace FormulaTelemetry.Sync.Core.Persistence;

public sealed partial class PostgresTelemetryStore
{
    public async Task UpsertSessionResultsAsync(IEnumerable<OpenF1SessionResult> results, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO session_results (
                session_key, driver_number, meeting_key, position, duration_json, gap_to_leader_json,
                number_of_laps, dnf, dns, dsq)
            VALUES (
                @session_key, @driver_number, @meeting_key, @position, @duration_json::jsonb, @gap_to_leader_json::jsonb,
                @number_of_laps, @dnf, @dns, @dsq)
            ON CONFLICT (session_key, driver_number) DO UPDATE SET
                meeting_key = EXCLUDED.meeting_key,
                position = EXCLUDED.position,
                duration_json = EXCLUDED.duration_json,
                gap_to_leader_json = EXCLUDED.gap_to_leader_json,
                number_of_laps = EXCLUDED.number_of_laps,
                dnf = EXCLUDED.dnf,
                dns = EXCLUDED.dns,
                dsq = EXCLUDED.dsq;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        foreach (var r in results)
        {
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("session_key", r.SessionKey);
            cmd.Parameters.AddWithValue("driver_number", r.DriverNumber);
            cmd.Parameters.AddWithValue("meeting_key", r.MeetingKey);
            cmd.Parameters.AddWithValue("position", (object?)r.Position ?? DBNull.Value);
            cmd.Parameters.AddWithValue("duration_json", (object?)JsonElementHelpers.ToJsonb(r.Duration) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("gap_to_leader_json", (object?)JsonElementHelpers.ToJsonb(r.GapToLeader) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("number_of_laps", (object?)r.NumberOfLaps ?? DBNull.Value);
            cmd.Parameters.AddWithValue("dnf", r.Dnf ?? false);
            cmd.Parameters.AddWithValue("dns", r.Dns ?? false);
            cmd.Parameters.AddWithValue("dsq", r.Dsq ?? false);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task UpsertStartingGridAsync(IEnumerable<OpenF1StartingGrid> entries, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO starting_grid_entries (session_key, driver_number, meeting_key, position, lap_duration)
            VALUES (@session_key, @driver_number, @meeting_key, @position, @lap_duration)
            ON CONFLICT (session_key, driver_number) DO UPDATE SET
                meeting_key = EXCLUDED.meeting_key,
                position = EXCLUDED.position,
                lap_duration = EXCLUDED.lap_duration;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        foreach (var e in entries)
        {
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("session_key", e.SessionKey);
            cmd.Parameters.AddWithValue("driver_number", e.DriverNumber);
            cmd.Parameters.AddWithValue("meeting_key", e.MeetingKey);
            cmd.Parameters.AddWithValue("position", e.Position);
            cmd.Parameters.AddWithValue("lap_duration", (object?)e.LapDuration ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task UpsertStintsAsync(IEnumerable<OpenF1Stint> stints, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO stints (
                session_key, driver_number, stint_number, meeting_key, lap_start, lap_end, compound, tyre_age_at_start)
            VALUES (
                @session_key, @driver_number, @stint_number, @meeting_key, @lap_start, @lap_end, @compound, @tyre_age_at_start)
            ON CONFLICT (session_key, driver_number, stint_number) DO UPDATE SET
                meeting_key = EXCLUDED.meeting_key,
                lap_start = EXCLUDED.lap_start,
                lap_end = EXCLUDED.lap_end,
                compound = EXCLUDED.compound,
                tyre_age_at_start = EXCLUDED.tyre_age_at_start;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        foreach (var s in stints)
        {
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("session_key", s.SessionKey);
            cmd.Parameters.AddWithValue("driver_number", s.DriverNumber);
            cmd.Parameters.AddWithValue("stint_number", s.StintNumber);
            cmd.Parameters.AddWithValue("meeting_key", s.MeetingKey);
            cmd.Parameters.AddWithValue("lap_start", (object?)s.LapStart ?? DBNull.Value);
            cmd.Parameters.AddWithValue("lap_end", (object?)s.LapEnd ?? DBNull.Value);
            cmd.Parameters.AddWithValue("compound", (object?)s.Compound ?? DBNull.Value);
            cmd.Parameters.AddWithValue("tyre_age_at_start", (object?)s.TyreAgeAtStart ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task UpsertPitsAsync(IEnumerable<OpenF1Pit> pits, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO pits (session_key, driver_number, lap_number, date, meeting_key, lane_duration, stop_duration)
            VALUES (@session_key, @driver_number, @lap_number, @date, @meeting_key, @lane_duration, @stop_duration)
            ON CONFLICT (session_key, driver_number, lap_number, date) DO UPDATE SET
                meeting_key = EXCLUDED.meeting_key,
                lane_duration = EXCLUDED.lane_duration,
                stop_duration = EXCLUDED.stop_duration;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        foreach (var p in pits)
        {
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("session_key", p.SessionKey);
            cmd.Parameters.AddWithValue("driver_number", p.DriverNumber);
            cmd.Parameters.AddWithValue("lap_number", p.LapNumber);
            cmd.Parameters.AddWithValue("date", p.Date);
            cmd.Parameters.AddWithValue("meeting_key", p.MeetingKey);
            cmd.Parameters.AddWithValue("lane_duration", (object?)p.LaneDuration ?? DBNull.Value);
            cmd.Parameters.AddWithValue("stop_duration", (object?)p.StopDuration ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task UpsertIntervalsAsync(IEnumerable<OpenF1Interval> intervals, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO intervals (session_key, driver_number, date, meeting_key, gap_to_leader_text, interval_text)
            VALUES (@session_key, @driver_number, @date, @meeting_key, @gap_to_leader_text, @interval_text)
            ON CONFLICT (session_key, driver_number, date) DO UPDATE SET
                meeting_key = EXCLUDED.meeting_key,
                gap_to_leader_text = EXCLUDED.gap_to_leader_text,
                interval_text = EXCLUDED.interval_text;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        foreach (var i in intervals)
        {
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("session_key", i.SessionKey);
            cmd.Parameters.AddWithValue("driver_number", i.DriverNumber);
            cmd.Parameters.AddWithValue("date", i.Date);
            cmd.Parameters.AddWithValue("meeting_key", i.MeetingKey);
            cmd.Parameters.AddWithValue("gap_to_leader_text", (object?)JsonElementHelpers.ToFlexibleText(i.GapToLeader) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("interval_text", (object?)JsonElementHelpers.ToFlexibleText(i.Interval) ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task UpsertPositionsAsync(IEnumerable<OpenF1Position> positions, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO positions (session_key, driver_number, date, meeting_key, position)
            VALUES (@session_key, @driver_number, @date, @meeting_key, @position)
            ON CONFLICT (session_key, driver_number, date) DO UPDATE SET
                meeting_key = EXCLUDED.meeting_key,
                position = EXCLUDED.position;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        foreach (var p in positions)
        {
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("session_key", p.SessionKey);
            cmd.Parameters.AddWithValue("driver_number", p.DriverNumber);
            cmd.Parameters.AddWithValue("date", p.Date);
            cmd.Parameters.AddWithValue("meeting_key", p.MeetingKey);
            cmd.Parameters.AddWithValue("position", p.Position);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task UpsertWeatherAsync(IEnumerable<OpenF1Weather> samples, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO weather_samples (
                session_key, date, meeting_key, air_temperature, track_temperature, humidity,
                pressure, rainfall, wind_direction, wind_speed)
            VALUES (
                @session_key, @date, @meeting_key, @air_temperature, @track_temperature, @humidity,
                @pressure, @rainfall, @wind_direction, @wind_speed)
            ON CONFLICT (session_key, date) DO UPDATE SET
                meeting_key = EXCLUDED.meeting_key,
                air_temperature = EXCLUDED.air_temperature,
                track_temperature = EXCLUDED.track_temperature,
                humidity = EXCLUDED.humidity,
                pressure = EXCLUDED.pressure,
                rainfall = EXCLUDED.rainfall,
                wind_direction = EXCLUDED.wind_direction,
                wind_speed = EXCLUDED.wind_speed;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        foreach (var w in samples)
        {
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("session_key", w.SessionKey);
            cmd.Parameters.AddWithValue("date", w.Date);
            cmd.Parameters.AddWithValue("meeting_key", w.MeetingKey);
            cmd.Parameters.AddWithValue("air_temperature", (object?)w.AirTemperature ?? DBNull.Value);
            cmd.Parameters.AddWithValue("track_temperature", (object?)w.TrackTemperature ?? DBNull.Value);
            cmd.Parameters.AddWithValue("humidity", (object?)w.Humidity ?? DBNull.Value);
            cmd.Parameters.AddWithValue("pressure", (object?)w.Pressure ?? DBNull.Value);
            cmd.Parameters.AddWithValue("rainfall", (object?)JsonElementHelpers.ToFlexibleBool(w.Rainfall) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("wind_direction", (object?)w.WindDirection ?? DBNull.Value);
            cmd.Parameters.AddWithValue("wind_speed", (object?)w.WindSpeed ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task UpsertRaceControlAsync(IEnumerable<OpenF1RaceControl> messages, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO race_control_messages (
                session_key, date, meeting_key, category, driver_number, flag, lap_number,
                message, qualifying_phase, scope, sector)
            VALUES (
                @session_key, @date, @meeting_key, @category, @driver_number, @flag, @lap_number,
                @message, @qualifying_phase, @scope, @sector)
            ON CONFLICT ON CONSTRAINT uq_race_control_natural DO UPDATE SET
                meeting_key = EXCLUDED.meeting_key,
                flag = EXCLUDED.flag,
                lap_number = EXCLUDED.lap_number,
                qualifying_phase = EXCLUDED.qualifying_phase,
                scope = EXCLUDED.scope,
                sector = EXCLUDED.sector;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        foreach (var m in messages)
        {
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("session_key", m.SessionKey);
            cmd.Parameters.AddWithValue("date", m.Date);
            cmd.Parameters.AddWithValue("meeting_key", m.MeetingKey);
            cmd.Parameters.AddWithValue("category", (object?)m.Category ?? DBNull.Value);
            cmd.Parameters.AddWithValue("driver_number", (object?)m.DriverNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("flag", (object?)m.Flag ?? DBNull.Value);
            cmd.Parameters.AddWithValue("lap_number", (object?)m.LapNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("message", (object?)m.Message ?? DBNull.Value);
            cmd.Parameters.AddWithValue("qualifying_phase", (object?)m.QualifyingPhase ?? DBNull.Value);
            cmd.Parameters.AddWithValue("scope", (object?)m.Scope ?? DBNull.Value);
            cmd.Parameters.AddWithValue("sector", (object?)m.Sector ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task UpsertOvertakesAsync(IEnumerable<OpenF1Overtake> overtakes, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO overtakes (
                session_key, date, overtaking_driver_number, overtaken_driver_number, meeting_key, position)
            VALUES (
                @session_key, @date, @overtaking_driver_number, @overtaken_driver_number, @meeting_key, @position)
            ON CONFLICT (session_key, date, overtaking_driver_number, overtaken_driver_number) DO UPDATE SET
                meeting_key = EXCLUDED.meeting_key,
                position = EXCLUDED.position;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        foreach (var o in overtakes)
        {
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("session_key", o.SessionKey);
            cmd.Parameters.AddWithValue("date", o.Date);
            cmd.Parameters.AddWithValue("overtaking_driver_number", o.OvertakingDriverNumber);
            cmd.Parameters.AddWithValue("overtaken_driver_number", o.OvertakenDriverNumber);
            cmd.Parameters.AddWithValue("meeting_key", o.MeetingKey);
            cmd.Parameters.AddWithValue("position", (object?)o.Position ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task UpsertTeamRadiosAsync(IEnumerable<OpenF1TeamRadio> radios, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO team_radios (session_key, driver_number, date, meeting_key, recording_url)
            VALUES (@session_key, @driver_number, @date, @meeting_key, @recording_url)
            ON CONFLICT (session_key, driver_number, date) DO UPDATE SET
                meeting_key = EXCLUDED.meeting_key,
                recording_url = EXCLUDED.recording_url;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        foreach (var r in radios)
        {
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("session_key", r.SessionKey);
            cmd.Parameters.AddWithValue("driver_number", r.DriverNumber);
            cmd.Parameters.AddWithValue("date", r.Date);
            cmd.Parameters.AddWithValue("meeting_key", r.MeetingKey);
            cmd.Parameters.AddWithValue("recording_url", (object?)r.RecordingUrl ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task UpsertChampionshipDriversAsync(IEnumerable<OpenF1ChampionshipDriver> rows, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO championship_driver_standings (
                session_key, driver_number, meeting_key, points_start, points_current, position_start, position_current)
            VALUES (
                @session_key, @driver_number, @meeting_key, @points_start, @points_current, @position_start, @position_current)
            ON CONFLICT (session_key, driver_number) DO UPDATE SET
                meeting_key = EXCLUDED.meeting_key,
                points_start = EXCLUDED.points_start,
                points_current = EXCLUDED.points_current,
                position_start = EXCLUDED.position_start,
                position_current = EXCLUDED.position_current;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        foreach (var r in rows)
        {
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("session_key", r.SessionKey);
            cmd.Parameters.AddWithValue("driver_number", r.DriverNumber);
            cmd.Parameters.AddWithValue("meeting_key", r.MeetingKey);
            cmd.Parameters.AddWithValue("points_start", (object?)r.PointsStart ?? DBNull.Value);
            cmd.Parameters.AddWithValue("points_current", (object?)r.PointsCurrent ?? DBNull.Value);
            cmd.Parameters.AddWithValue("position_start", (object?)r.PositionStart ?? DBNull.Value);
            cmd.Parameters.AddWithValue("position_current", (object?)r.PositionCurrent ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task UpsertChampionshipTeamsAsync(IEnumerable<OpenF1ChampionshipTeam> rows, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO championship_team_standings (
                session_key, team_name, meeting_key, points_start, points_current, position_start, position_current)
            VALUES (
                @session_key, @team_name, @meeting_key, @points_start, @points_current, @position_start, @position_current)
            ON CONFLICT (session_key, team_name) DO UPDATE SET
                meeting_key = EXCLUDED.meeting_key,
                points_start = EXCLUDED.points_start,
                points_current = EXCLUDED.points_current,
                position_start = EXCLUDED.position_start,
                position_current = EXCLUDED.position_current;
            """;

        await using var conn = await OpenAsync(cancellationToken);
        foreach (var r in rows)
        {
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("session_key", r.SessionKey);
            cmd.Parameters.AddWithValue("team_name", r.TeamName);
            cmd.Parameters.AddWithValue("meeting_key", r.MeetingKey);
            cmd.Parameters.AddWithValue("points_start", (object?)r.PointsStart ?? DBNull.Value);
            cmd.Parameters.AddWithValue("points_current", (object?)r.PointsCurrent ?? DBNull.Value);
            cmd.Parameters.AddWithValue("position_start", (object?)r.PositionStart ?? DBNull.Value);
            cmd.Parameters.AddWithValue("position_current", (object?)r.PositionCurrent ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
