using FormulaTelemetry.Sync.Core.Configuration;
using FormulaTelemetry.Sync.Core.Models;
using FormulaTelemetry.Sync.Core.Services;
using Microsoft.Extensions.Options;

namespace FormulaTelemetry.Sync.Worker;

public sealed class SyncPendingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<SyncWorkerOptions> _options;
    private readonly ILogger<SyncPendingWorker> _logger;

    public SyncPendingWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<SyncWorkerOptions> options,
        ILogger<SyncPendingWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = _options.Value;
        var interval = TimeSpan.FromMinutes(Math.Max(1, opts.IntervalMinutes));

        if (opts.RunPendingOnStartup)
        {
            await RunDiscoverAsync(stoppingToken);
        }

        using var timer = new PeriodicTimer(interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunDiscoverAsync(stoppingToken);
        }
    }

    private async Task RunDiscoverAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var sync = scope.ServiceProvider.GetRequiredService<IOpenF1SyncService>();
            var email = scope.ServiceProvider.GetRequiredService<IEmailNotifier>();

            _logger.LogInformation("Worker: starting auto-discover sync…");
            var result = await sync.RunAsync(SyncRequest.ForAutoDiscover(), cancellationToken);

            foreach (var message in result.Messages)
            {
                _logger.LogInformation("{Message}", message);
            }

            if (!result.Success)
            {
                _logger.LogError("Worker sync failed: {Error}", result.Error);
                await TrySendEmailAsync(
                    email,
                    "FormulaTelemetry: sync failed",
                    $"Auto-discover sync failed at {DateTimeOffset.Now:u}.\n\nError: {result.Error}",
                    cancellationToken);
            }
            else
            {
                _logger.LogInformation(
                    "Worker sync done. Sessions={Sessions}, Laps={Laps}",
                    result.SessionsProcessed,
                    result.LapsUpserted);

                if (result.SessionsProcessed > 0)
                {
                    await TrySendEmailAsync(
                        email,
                        "FormulaTelemetry: new data downloaded",
                        $"""
                        New OpenF1 data synced at {DateTimeOffset.Now:u}.

                        Sessions processed: {result.SessionsProcessed}
                        Meetings upserted: {result.MeetingsUpserted}
                        Drivers upserted: {result.DriversUpserted}
                        Laps upserted: {result.LapsUpserted}
                        """,
                        cancellationToken);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // shutting down
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Worker sync iteration failed.");
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var email = scope.ServiceProvider.GetRequiredService<IEmailNotifier>();
                await TrySendEmailAsync(
                    email,
                    "FormulaTelemetry: sync exception",
                    $"Worker sync iteration failed at {DateTimeOffset.Now:u}.\n\n{ex}",
                    CancellationToken.None);
            }
            catch (Exception mailEx)
            {
                _logger.LogError(mailEx, "Failed to send error notification email.");
            }
        }
    }

    private async Task TrySendEmailAsync(
        IEmailNotifier email,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        try
        {
            await email.SendAsync(subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email: {Subject}", subject);
        }
    }
}
