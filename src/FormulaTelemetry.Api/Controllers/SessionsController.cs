using FormulaTelemetry.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace FormulaTelemetry.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SessionsController : ControllerBase
{
    private readonly ITelemetryReadStore _store;

    public SessionsController(ITelemetryReadStore store)
    {
        _store = store;
    }

    [HttpGet("{sessionKey:int}")]
    public async Task<IActionResult> GetByKey(int sessionKey, CancellationToken cancellationToken)
    {
        var session = await _store.GetSessionAsync(sessionKey, cancellationToken);
        return session is null ? NotFound() : Ok(session);
    }

    [HttpGet("{sessionKey:int}/results")]
    public async Task<IActionResult> GetResults(int sessionKey, CancellationToken cancellationToken)
    {
        var session = await _store.GetSessionAsync(sessionKey, cancellationToken);
        if (session is null)
        {
            return NotFound();
        }

        var results = await _store.GetSessionResultsAsync(sessionKey, cancellationToken);
        return Ok(results);
    }

    [HttpGet("{sessionKey:int}/laps")]
    public async Task<IActionResult> GetLaps(int sessionKey, CancellationToken cancellationToken)
    {
        var session = await _store.GetSessionAsync(sessionKey, cancellationToken);
        if (session is null)
        {
            return NotFound();
        }

        var laps = await _store.GetLapsAsync(sessionKey, cancellationToken);
        return Ok(laps);
    }
}
