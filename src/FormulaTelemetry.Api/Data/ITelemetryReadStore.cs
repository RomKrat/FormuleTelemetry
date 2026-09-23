using FormulaTelemetry.Api.Models;

namespace FormulaTelemetry.Api.Data;

public interface ITelemetryReadStore
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetAvailableYearsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MeetingDto>> GetMeetingsByYearAsync(int year, CancellationToken cancellationToken = default);

    Task<MeetingDto?> GetMeetingAsync(int meetingKey, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SessionDto>> GetSessionsByMeetingAsync(int meetingKey, CancellationToken cancellationToken = default);

    Task<SessionDto?> GetSessionAsync(int sessionKey, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SessionResultDto>> GetSessionResultsAsync(int sessionKey, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LapDto>> GetLapsAsync(int sessionKey, CancellationToken cancellationToken = default);
}
