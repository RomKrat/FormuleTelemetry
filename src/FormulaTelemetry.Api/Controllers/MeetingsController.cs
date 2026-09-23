using FormulaTelemetry.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace FormulaTelemetry.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MeetingsController : ControllerBase
{
    private readonly ITelemetryReadStore _store;

    public MeetingsController(ITelemetryReadStore store)
    {
        _store = store;
    }

    /// <summary>Distinct season years present in the database (newest first).</summary>
    [HttpGet("years")]
    public async Task<IActionResult> GetYears(CancellationToken cancellationToken)
    {
        var years = await _store.GetAvailableYearsAsync(cancellationToken);
        return Ok(years);
    }

    /// <summary>List meetings for a season year.</summary>
    [HttpGet]
    public async Task<IActionResult> GetByYear([FromQuery] int year, CancellationToken cancellationToken)
    {
        if (year < 2023 || year > 2100)
        {
            return BadRequest("Query parameter 'year' must be a valid F1 season year (>= 2023).");
        }

        var meetings = await _store.GetMeetingsByYearAsync(year, cancellationToken);
        return Ok(meetings);
    }

    [HttpGet("{meetingKey:int}")]
    public async Task<IActionResult> GetByKey(int meetingKey, CancellationToken cancellationToken)
    {
        var meeting = await _store.GetMeetingAsync(meetingKey, cancellationToken);
        return meeting is null ? NotFound() : Ok(meeting);
    }

    [HttpGet("{meetingKey:int}/sessions")]
    public async Task<IActionResult> GetSessions(int meetingKey, CancellationToken cancellationToken)
    {
        var meeting = await _store.GetMeetingAsync(meetingKey, cancellationToken);
        if (meeting is null)
        {
            return NotFound();
        }

        var sessions = await _store.GetSessionsByMeetingAsync(meetingKey, cancellationToken);
        return Ok(sessions);
    }
}
