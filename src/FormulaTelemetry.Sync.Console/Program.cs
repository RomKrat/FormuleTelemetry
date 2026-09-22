using FormulaTelemetry.Sync.Core;
using FormulaTelemetry.Sync.Core.Models;
using FormulaTelemetry.Sync.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddEnvironmentVariables();

builder.Services.ConfigureFormulaTelemetrySync(builder.Configuration);
builder.Services.AddFormulaTelemetrySync();

using var host = builder.Build();
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Sync.Console");

var request = ParseArgs(args);
if (request is null)
{
    PrintUsage();
    return 1;
}

logger.LogInformation(
    "Starting sync: Year={Year}, Round={Round}, Meeting={Meeting}, Session={Session}, Latest={Latest}, Discover={Discover}, Pending={Pending}",
    request.Year, request.Round, request.MeetingKey, request.SessionKey, request.Latest, request.AutoDiscover, request.PendingOnly);

var sync = host.Services.GetRequiredService<IOpenF1SyncService>();
var result = await sync.RunAsync(request);

foreach (var message in result.Messages)
{
    Console.WriteLine(message);
}

if (!result.Success)
{
    Console.Error.WriteLine(result.Error);
    return 2;
}

Console.WriteLine(
    $"Done. Sessions={result.SessionsProcessed}, Meetings={result.MeetingsUpserted}, Drivers={result.DriversUpserted}, Laps={result.LapsUpserted}");
return 0;

static SyncRequest? ParseArgs(string[] args)
{
    int? year = null;
    int? round = null;
    int? meeting = null;
    int? session = null;
    var pending = false;
    var latest = false;
    var discover = false;

    for (var i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--year" when i + 1 < args.Length && int.TryParse(args[++i], out var y):
                year = y;
                break;
            case "--round" when i + 1 < args.Length && int.TryParse(args[++i], out var r):
                round = r;
                break;
            case "--meeting" when i + 1 < args.Length && int.TryParse(args[++i], out var m):
                meeting = m;
                break;
            case "--session" when i + 1 < args.Length && int.TryParse(args[++i], out var s):
                session = s;
                break;
            case "--latest":
                latest = true;
                break;
            case "--discover":
                discover = true;
                break;
            case "--pending":
                pending = true;
                break;
            case "--help" or "-h":
                return null;
        }
    }

    if (round is not null && year is null)
    {
        Console.Error.WriteLine("--round requires --year.");
        return null;
    }

    // Priority: --session > --latest > --meeting > (--year + --round) > --discover > --year > --pending
    if (session is not null)
    {
        return SyncRequest.ForSession(session.Value);
    }

    if (latest)
    {
        return SyncRequest.ForLatest();
    }

    if (meeting is not null)
    {
        return SyncRequest.ForMeeting(meeting.Value);
    }

    if (year is not null && round is not null)
    {
        return SyncRequest.ForYearRound(year.Value, round.Value);
    }

    if (discover)
    {
        return SyncRequest.ForAutoDiscover();
    }

    if (year is not null)
    {
        return SyncRequest.ForYear(year.Value);
    }

    if (pending)
    {
        return SyncRequest.Pending();
    }

    return null;
}

static void PrintUsage()
{
    Console.WriteLine("""
        FormulaTelemetry.Sync.Console — download OpenF1 data into PostgreSQL

        Usage:
          --meeting <key>           Sync entire race weekend (all ended sessions)
          --year <yyyy> --round <n> Sync n-th meeting of the season (1-based)
          --year <yyyy>             Sync all ended sessions for a season
          --session <key>           Sync one session by OpenF1 session_key
          --latest                  Sync the latest OpenF1 session
          --discover                Auto-discover ended sessions not yet Completed (Worker mode)
          --pending                 Sync sessions already in DB that are not Completed

        Examples:
          dotnet run -- --meeting 1294
          dotnet run -- --year 2026 --round 2
          dotnet run -- --discover
          dotnet run -- --latest
          dotnet run -- --session 9158
          dotnet run -- --pending
        """);
}
