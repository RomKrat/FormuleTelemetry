using FormulaTelemetry.Sync.Core.Configuration;
using FormulaTelemetry.Sync.Core.Models;
using FormulaTelemetry.Sync.Core.OpenF1;
using FormulaTelemetry.Sync.Core.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FormulaTelemetry.Sync.Core.Services;

public interface IOpenF1SyncService
{
    Task<SyncResult> RunAsync(SyncRequest request, CancellationToken cancellationToken = default);
}

public sealed class OpenF1SyncService : IOpenF1SyncService
{
    private static readonly TimeSpan RequestGap = TimeSpan.FromMilliseconds(400);

    private readonly IOpenF1Client _openF1;
    private readonly ITelemetryStore _store;
    private readonly SyncWorkerOptions _workerOptions;
    private readonly ILogger<OpenF1SyncService> _logger;

    public OpenF1SyncService(
        IOpenF1Client openF1,
        ITelemetryStore store,
        IOptions<SyncWorkerOptions> workerOptions,
        ILogger<OpenF1SyncService> logger)
    {
        _openF1 = openF1;
        _store = store;
        _workerOptions = workerOptions.Value;
        _logger = logger;
    }

    public async Task<SyncResult> RunAsync(SyncRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.SessionKey is int sessionKey)
            {
                return await SyncSessionsAsync([sessionKey], ensureMeetingFromOpenF1: true, cancellationToken);
            }

            if (request.Latest)
            {
                var latestSessions = await _openF1.GetAsync<OpenF1Session>(
                    "sessions?session_key=latest",
                    cancellationToken);
                var latest = latestSessions.FirstOrDefault();
                if (latest is null)
                {
                    return SyncResult.Fail("OpenF1 returned no latest session.");
                }

                _logger.LogInformation(
                    "Latest session: {SessionKey} ({SessionName})",
                    latest.SessionKey,
                    latest.SessionName);
                return await SyncSessionsAsync([latest.SessionKey], ensureMeetingFromOpenF1: true, cancellationToken);
            }

            if (request.MeetingKey is int meetingKey)
            {
                return await SyncMeetingAsync(meetingKey, cancellationToken);
            }

            if (request.Year is int year && request.Round is int round)
            {
                if (round < 1)
                {
                    return SyncResult.Fail("--round must be >= 1.");
                }

                var meetings = await _openF1.GetAsync<OpenF1Meeting>($"meetings?year={year}", cancellationToken);
                var ordered = meetings
                    .Where(m => m.IsCancelled != true)
                    .OrderBy(m => m.DateStart ?? DateTimeOffset.MaxValue)
                    .ToList();

                if (round > ordered.Count)
                {
                    return SyncResult.Fail(
                        $"Year {year} has only {ordered.Count} meetings; --round {round} is out of range.");
                }

                var meeting = ordered[round - 1];
                _logger.LogInformation(
                    "Year {Year} round {Round}: meeting_key={MeetingKey} ({MeetingName})",
                    year,
                    round,
                    meeting.MeetingKey,
                    meeting.MeetingName);
                return await SyncMeetingAsync(meeting.MeetingKey, cancellationToken);
            }

            if (request.AutoDiscover)
            {
                return await AutoDiscoverAndSyncAsync(cancellationToken);
            }

            if (request.Year is int fullYear)
            {
                var sessions = await _openF1.GetAsync<OpenF1Session>($"sessions?year={fullYear}", cancellationToken);
                var ended = sessions
                    .Where(s => s.DateEnd is not null && s.DateEnd < DateTimeOffset.UtcNow)
                    .Where(s => s.IsCancelled != true)
                    .Select(s => s.SessionKey)
                    .Distinct()
                    .ToList();

                _logger.LogInformation("Year {Year}: {Count} ended sessions to sync.", fullYear, ended.Count);
                return await SyncSessionsAsync(ended, ensureMeetingFromOpenF1: true, cancellationToken);
            }

            if (request.PendingOnly)
            {
                var pending = await _store.GetPendingSessionKeysAsync(cancellationToken);
                _logger.LogInformation("Pending sync: {Count} sessions.", pending.Count);
                if (pending.Count == 0)
                {
                    return new SyncResult
                    {
                        Success = true,
                        Messages = ["No pending sessions."]
                    };
                }

                return await SyncSessionsAsync(pending, ensureMeetingFromOpenF1: true, cancellationToken);
            }

            return SyncResult.Fail(
                "Specify --year, --year/--round, --meeting, --session, --latest, --discover, or --pending.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync failed.");
            return SyncResult.Fail(ex.Message);
        }
    }

    private async Task<SyncResult> AutoDiscoverAndSyncAsync(CancellationToken cancellationToken)
    {
        var yearsBack = Math.Max(0, _workerOptions.DiscoverYearsBack);
        var currentYear = DateTime.UtcNow.Year;
        var years = Enumerable.Range(currentYear - yearsBack, yearsBack + 1).ToList();

        var completed = await _store.GetCompletedSessionKeysAsync(cancellationToken);
        var toSync = new List<(int SessionKey, DateTimeOffset? DateEnd)>();

        foreach (var year in years)
        {
            var sessions = await GetAsync<OpenF1Session>($"sessions?year={year}", cancellationToken);
            foreach (var session in sessions)
            {
                if (session.IsCancelled == true)
                {
                    continue;
                }

                if (session.DateEnd is null || session.DateEnd >= DateTimeOffset.UtcNow)
                {
                    continue;
                }

                if (completed.Contains(session.SessionKey))
                {
                    continue;
                }

                toSync.Add((session.SessionKey, session.DateEnd));
            }
        }

        var keys = toSync
            .OrderBy(x => x.DateEnd)
            .Select(x => x.SessionKey)
            .Distinct()
            .ToList();

        _logger.LogInformation(
            "Auto-discover years {Years}: {Count} ended sessions not Completed.",
            string.Join(",", years),
            keys.Count);

        if (keys.Count == 0)
        {
            return new SyncResult
            {
                Success = true,
                Messages = ["Auto-discover: nothing to sync."]
            };
        }

        return await SyncSessionsAsync(keys, ensureMeetingFromOpenF1: true, cancellationToken);
    }

    private async Task<SyncResult> SyncMeetingAsync(int meetingKey, CancellationToken cancellationToken)
    {
        var meetingList = await GetAsync<OpenF1Meeting>($"meetings?meeting_key={meetingKey}", cancellationToken);
        var meeting = meetingList.FirstOrDefault();
        if (meeting is null)
        {
            return SyncResult.Fail($"Meeting {meetingKey} not found on OpenF1.");
        }

        await _store.UpsertMeetingAsync(meeting, cancellationToken);

        var sessions = await GetAsync<OpenF1Session>($"sessions?meeting_key={meetingKey}", cancellationToken);
        var endedKeys = sessions
            .Where(s => s.IsCancelled != true)
            .Where(s => s.DateEnd is not null && s.DateEnd < DateTimeOffset.UtcNow)
            .OrderBy(s => s.DateStart ?? DateTimeOffset.MaxValue)
            .Select(s => s.SessionKey)
            .Distinct()
            .ToList();

        _logger.LogInformation(
            "Meeting {MeetingKey} ({MeetingName}): {Count} ended sessions to sync.",
            meetingKey,
            meeting.MeetingName,
            endedKeys.Count);

        if (endedKeys.Count == 0)
        {
            return new SyncResult
            {
                Success = true,
                MeetingsUpserted = 1,
                Messages =
                [
                    $"Meeting {meetingKey} ({meeting.MeetingName}): no ended sessions yet."
                ]
            };
        }

        // Meeting already upserted — skip re-fetch per session
        var result = await SyncSessionsAsync(endedKeys, ensureMeetingFromOpenF1: false, cancellationToken);
        return new SyncResult
        {
            Success = result.Success,
            SessionsProcessed = result.SessionsProcessed,
            MeetingsUpserted = result.MeetingsUpserted + 1,
            DriversUpserted = result.DriversUpserted,
            LapsUpserted = result.LapsUpserted,
            Messages = result.Messages,
            Error = result.Error
        };
    }

    private async Task<SyncResult> SyncSessionsAsync(
        IReadOnlyList<int> sessionKeys,
        bool ensureMeetingFromOpenF1,
        CancellationToken cancellationToken)
    {
        var messages = new List<string>();
        var meetings = 0;
        var drivers = 0;
        var laps = 0;
        var processed = 0;

        foreach (var sessionKey in sessionKeys)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (processed > 0)
            {
                var pause = TimeSpan.FromSeconds(Math.Max(0, _workerOptions.PauseBetweenSessionsSeconds));
                if (pause > TimeSpan.Zero)
                {
                    await Task.Delay(pause, cancellationToken);
                }
            }

            try
            {
                var sessionList = await GetAsync<OpenF1Session>($"sessions?session_key={sessionKey}", cancellationToken);
                var session = sessionList.FirstOrDefault();
                if (session is null)
                {
                    messages.Add($"Session {sessionKey}: not found on OpenF1.");
                    continue;
                }

                if (ensureMeetingFromOpenF1)
                {
                    var meetingList = await GetAsync<OpenF1Meeting>(
                        $"meetings?meeting_key={session.MeetingKey}",
                        cancellationToken);
                    var meeting = meetingList.FirstOrDefault();
                    if (meeting is not null)
                    {
                        await _store.UpsertMeetingAsync(meeting, cancellationToken);
                        meetings++;
                    }
                }

                await _store.UpsertSessionAsync(session, cancellationToken);
                await _store.MarkSyncRunningAsync(sessionKey, cancellationToken);

                var driverList = await GetAsync<OpenF1Driver>($"drivers?session_key={sessionKey}", cancellationToken);
                await _store.UpsertDriversAsync(driverList, cancellationToken);
                drivers += driverList.Count;

                var lapList = await GetAsync<OpenF1Lap>($"laps?session_key={sessionKey}", cancellationToken);
                await _store.UpsertLapsAsync(lapList, cancellationToken);
                laps += lapList.Count;

                var results = await GetAsync<OpenF1SessionResult>($"session_result?session_key={sessionKey}", cancellationToken);
                await _store.UpsertSessionResultsAsync(results, cancellationToken);

                var grid = await GetAsync<OpenF1StartingGrid>($"starting_grid?session_key={sessionKey}", cancellationToken);
                await _store.UpsertStartingGridAsync(grid, cancellationToken);

                var stints = await GetAsync<OpenF1Stint>($"stints?session_key={sessionKey}", cancellationToken);
                await _store.UpsertStintsAsync(stints, cancellationToken);

                var pits = await GetAsync<OpenF1Pit>($"pit?session_key={sessionKey}", cancellationToken);
                await _store.UpsertPitsAsync(pits, cancellationToken);

                var intervals = await GetAsync<OpenF1Interval>($"intervals?session_key={sessionKey}", cancellationToken);
                await _store.UpsertIntervalsAsync(intervals, cancellationToken);

                var positions = await GetAsync<OpenF1Position>($"position?session_key={sessionKey}", cancellationToken);
                await _store.UpsertPositionsAsync(positions, cancellationToken);

                var weather = await GetAsync<OpenF1Weather>($"weather?session_key={sessionKey}", cancellationToken);
                await _store.UpsertWeatherAsync(weather, cancellationToken);

                var raceControl = await GetAsync<OpenF1RaceControl>($"race_control?session_key={sessionKey}", cancellationToken);
                await _store.UpsertRaceControlAsync(raceControl, cancellationToken);

                var overtakes = await GetAsync<OpenF1Overtake>($"overtakes?session_key={sessionKey}", cancellationToken);
                await _store.UpsertOvertakesAsync(overtakes, cancellationToken);

                var radios = await GetAsync<OpenF1TeamRadio>($"team_radio?session_key={sessionKey}", cancellationToken);
                await _store.UpsertTeamRadiosAsync(radios, cancellationToken);

                var isRace = string.Equals(session.SessionType, "Race", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(session.SessionName, "Race", StringComparison.OrdinalIgnoreCase);
                var champDrivers = 0;
                var champTeams = 0;
                if (isRace)
                {
                    var cd = await GetAsync<OpenF1ChampionshipDriver>(
                        $"championship_drivers?session_key={sessionKey}",
                        cancellationToken);
                    await _store.UpsertChampionshipDriversAsync(cd, cancellationToken);
                    champDrivers = cd.Count;

                    var ct = await GetAsync<OpenF1ChampionshipTeam>(
                        $"championship_teams?session_key={sessionKey}",
                        cancellationToken);
                    await _store.UpsertChampionshipTeamsAsync(ct, cancellationToken);
                    champTeams = ct.Count;
                }

                await _store.MarkSyncCompletedAsync(sessionKey, lapList.Count, cancellationToken);
                processed++;
                messages.Add(
                    $"Session {sessionKey} ({session.SessionName}): drivers={driverList.Count}, laps={lapList.Count}, " +
                    $"results={results.Count}, grid={grid.Count}, stints={stints.Count}, pits={pits.Count}, " +
                    $"intervals={intervals.Count}, positions={positions.Count}, weather={weather.Count}, " +
                    $"race_control={raceControl.Count}, overtakes={overtakes.Count}, radios={radios.Count}, " +
                    $"champ_drivers={champDrivers}, champ_teams={champTeams}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed syncing session {SessionKey}", sessionKey);
                await _store.MarkSyncFailedAsync(sessionKey, ex.Message, cancellationToken);
                messages.Add($"Session {sessionKey}: FAILED — {ex.Message}");
            }
        }

        return new SyncResult
        {
            Success = true,
            SessionsProcessed = processed,
            MeetingsUpserted = meetings,
            DriversUpserted = drivers,
            LapsUpserted = laps,
            Messages = messages
        };
    }

    private async Task<IReadOnlyList<T>> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        await Task.Delay(RequestGap, cancellationToken);
        return await _openF1.GetAsync<T>(path, cancellationToken);
    }
}

