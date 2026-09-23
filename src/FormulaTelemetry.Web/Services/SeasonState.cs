using FormulaTelemetry.Web.Helpers;
using FormulaTelemetry.Web.Models;

namespace FormulaTelemetry.Web.Services;

/// <summary>Shared season selection + meetings for top bar and sidebar.</summary>
public sealed class SeasonState
{
    private readonly TelemetryApiClient _api;
    private bool _initialized;

    public SeasonState(TelemetryApiClient api)
    {
        _api = api;
    }

    public int Year { get; private set; } = DateTime.UtcNow.Year;
    public IReadOnlyList<int> AvailableYears { get; private set; } = [];
    public IReadOnlyList<MeetingDto> Meetings { get; private set; } = [];
    public bool IsLoading { get; private set; }
    public string? Error { get; private set; }
    public int? SelectedMeetingKey { get; private set; }

    public event Action? Changed;

    public void SelectMeeting(int? meetingKey)
    {
        if (SelectedMeetingKey == meetingKey) return;
        SelectedMeetingKey = meetingKey;
        Notify();
    }

    public MeetingDto? LastCompleted =>
        Meetings.LastOrDefault(m => MeetingCalendar.IsCompleted(m));

    public MeetingDto? NextUpcoming =>
        Meetings.FirstOrDefault(m => !m.IsCancelled && m.DateStart is not null && !MeetingCalendar.IsCompleted(m));

    public async Task EnsureInitializedAsync()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            AvailableYears = await _api.GetYearsAsync() ?? [];
            if (AvailableYears.Count == 0)
            {
                AvailableYears = [DateTime.UtcNow.Year];
            }

            var current = DateTime.UtcNow.Year;
            Year = AvailableYears.Contains(current) ? current : AvailableYears[0];
            await LoadMeetingsAsync();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            AvailableYears = [DateTime.UtcNow.Year];
            Notify();
        }
    }

    public async Task SetYearAsync(int year)
    {
        if (year == Year && Meetings.Count > 0) return;
        Year = year;
        await LoadMeetingsAsync();
    }

    private async Task LoadMeetingsAsync()
    {
        IsLoading = true;
        Error = null;
        Notify();
        try
        {
            Meetings = await _api.GetMeetingsAsync(Year) ?? [];
        }
        catch (Exception ex)
        {
            Meetings = [];
            Error = ex.Message;
        }
        finally
        {
            IsLoading = false;
            Notify();
        }
    }

    private void Notify() => Changed?.Invoke();
}
