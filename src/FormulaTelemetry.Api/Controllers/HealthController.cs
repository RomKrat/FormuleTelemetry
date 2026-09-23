using FormulaTelemetry.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace FormulaTelemetry.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    private readonly ITelemetryReadStore _store;

    public HealthController(ITelemetryReadStore store)
    {
        _store = store;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            var ok = await _store.CanConnectAsync(cancellationToken);
            return Ok(new { status = ok ? "Healthy" : "Degraded", database = ok });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "Unhealthy",
                database = false,
                error = ex.Message
            });
        }
    }
}
