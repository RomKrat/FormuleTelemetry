using System.Net.Http.Json;
using FormulaTelemetry.Web.Models;

namespace FormulaTelemetry.Web.Services;

public sealed class TelemetryApiClient
{
    private readonly HttpClient _http;

    public TelemetryApiClient(HttpClient http)
    {
        _http = http;
    }

    public Task<List<int>?> GetYearsAsync(CancellationToken cancellationToken = default) =>
        _http.GetFromJsonAsync<List<int>>("api/meetings/years", cancellationToken);

    public Task<List<MeetingDto>?> GetMeetingsAsync(int year, CancellationToken cancellationToken = default) =>
        _http.GetFromJsonAsync<List<MeetingDto>>($"api/meetings?year={year}", cancellationToken);

    public Task<MeetingDto?> GetMeetingAsync(int meetingKey, CancellationToken cancellationToken = default) =>
        _http.GetFromJsonAsync<MeetingDto>($"api/meetings/{meetingKey}", cancellationToken);

    public Task<List<SessionDto>?> GetSessionsAsync(int meetingKey, CancellationToken cancellationToken = default) =>
        _http.GetFromJsonAsync<List<SessionDto>>($"api/meetings/{meetingKey}/sessions", cancellationToken);

    public Task<SessionDto?> GetSessionAsync(int sessionKey, CancellationToken cancellationToken = default) =>
        _http.GetFromJsonAsync<SessionDto>($"api/sessions/{sessionKey}", cancellationToken);

    public Task<List<SessionResultDto>?> GetResultsAsync(int sessionKey, CancellationToken cancellationToken = default) =>
        _http.GetFromJsonAsync<List<SessionResultDto>>($"api/sessions/{sessionKey}/results", cancellationToken);

    public Task<List<LapDto>?> GetLapsAsync(int sessionKey, CancellationToken cancellationToken = default) =>
        _http.GetFromJsonAsync<List<LapDto>>($"api/sessions/{sessionKey}/laps", cancellationToken);

    public Task<List<StintDto>?> GetStintsAsync(int sessionKey, CancellationToken cancellationToken = default) =>
        _http.GetFromJsonAsync<List<StintDto>>($"api/sessions/{sessionKey}/stints", cancellationToken);

    public Task<List<QualifyingPhaseDto>?> GetQualifyingPhasesAsync(int sessionKey, CancellationToken cancellationToken = default) =>
        _http.GetFromJsonAsync<List<QualifyingPhaseDto>>($"api/sessions/{sessionKey}/qualifying-phases", cancellationToken);

    public Task<List<StartingGridEntryDto>?> GetStartingGridAsync(int sessionKey, CancellationToken cancellationToken = default) =>
        _http.GetFromJsonAsync<List<StartingGridEntryDto>>($"api/sessions/{sessionKey}/starting-grid", cancellationToken);

    public Task<List<LapPositionDto>?> GetLapPositionsAsync(int sessionKey, CancellationToken cancellationToken = default) =>
        _http.GetFromJsonAsync<List<LapPositionDto>>($"api/sessions/{sessionKey}/lap-positions", cancellationToken);
}
