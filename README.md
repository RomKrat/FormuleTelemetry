# FormulaTelemetry

International F1 post-session telemetry platform (OpenF1 → PostgreSQL → Web API → Blazor / MAUI).

## Solution layout

| Project | Type | Role |
|---------|------|------|
| `FormulaTelemetry.Sync.Core` | Class library | Shared OpenF1 download + PostgreSQL upsert |
| `FormulaTelemetry.Sync.Console` | Console | One-shot sync |
| `FormulaTelemetry.Sync.Worker` | Worker Service | Auto-discovers ended sessions and syncs them periodically |
| `FormulaTelemetry.Api` | ASP.NET Core Web API | Read-only middle tier for Blazor / MAUI (IIS later) |
| `FormulaTelemetry.Web` | Blazor WebAssembly | Browser UI (calls Web API only) |

Database: local PostgreSQL `formulatelemetry` — see [docs/database.md](docs/database.md).

Architecture diagrams (system, components, DB ER): [docs/architecture.md](docs/architecture.md).

UI design (web + mobile, brand, themes, screens): [docs/ui-design.md](docs/ui-design.md).

## Web (Blazor WASM)

Needs **API running** at the same time (CORS + `ApiBaseUrl`).

```powershell
# Terminal 1
dotnet run --project src/FormulaTelemetry.Api --launch-profile https

# Terminal 2
dotnet run --project src/FormulaTelemetry.Web --launch-profile https
```

Or in Visual Studio: right-click solution → **Configure Startup Projects** → multiple projects → start **Api** and **Web**.

- Web: `https://localhost:7236`
- API: `https://localhost:7245` (configure in `src/FormulaTelemetry.Web/wwwroot/appsettings.json` → `ApiBaseUrl`)

Pages: Season (meetings) → Meeting (sessions) → Session (results / laps).

## Web API

```powershell
cd C:\Roman\FormuleTelemetry
dotnet run --project src/FormulaTelemetry.Api
```

Swagger UI: `http://localhost:5208/swagger`

| Method | Route | Meaning |
|--------|-------|---------|
| GET | `/api/health` | API + DB ping |
| GET | `/api/meetings?year={yyyy}` | Meetings in a season |
| GET | `/api/meetings/{meetingKey}` | Meeting detail |
| GET | `/api/meetings/{meetingKey}/sessions` | Sessions of a weekend |
| GET | `/api/sessions/{sessionKey}` | Session detail |
| GET | `/api/sessions/{sessionKey}/results` | Session results (+ driver/team names) |
| GET | `/api/sessions/{sessionKey}/laps` | Laps for a session |

Connection string: `src/FormulaTelemetry.Api/appsettings.json` → `Database:ConnectionString`. Does **not** sync OpenF1 — only reads PostgreSQL.

## Sync.Console

```powershell
cd C:\Roman\FormuleTelemetry
dotnet run --project src/FormulaTelemetry.Sync.Console -- --meeting 1294
dotnet run --project src/FormulaTelemetry.Sync.Console -- --year 2026 --round 2
dotnet run --project src/FormulaTelemetry.Sync.Console -- --latest
dotnet run --project src/FormulaTelemetry.Sync.Console -- --session 9158
dotnet run --project src/FormulaTelemetry.Sync.Console -- --year 2024
dotnet run --project src/FormulaTelemetry.Sync.Console -- --discover
dotnet run --project src/FormulaTelemetry.Sync.Console -- --pending
```

| Argument | Meaning |
|----------|---------|
| `--meeting <key>` | Entire race weekend (all ended sessions of that meeting) |
| `--year` + `--round` | N-th meeting of the season (1-based calendar order) |
| `--year` | All ended sessions in the season |
| `--session` | Single session |
| `--latest` | Latest OpenF1 session |
| `--discover` | Ended OpenF1 sessions not yet `Completed` in `sync_jobs` (same as Worker) |
| `--pending` | Incomplete sync jobs already in DB |

Connection string: `src/FormulaTelemetry.Sync.Console/appsettings.json` → `Database:ConnectionString`.

Each session sync loads the full pack (results, grid, stints, pits, intervals, weather, race control, …). Telemetry tables `car_data` / `locations` are not filled yet.

## Sync.Worker

### Run from Visual Studio / CLI

Set **FormulaTelemetry.Sync.Worker** as Startup Project and press F5, or:

```powershell
dotnet run --project src/FormulaTelemetry.Sync.Worker
```

On each tick (and optionally on startup) the Worker:

1. Loads ended sessions from OpenF1 for the current year and `DiscoverYearsBack` previous years
2. Skips sessions already `Completed` in `sync_jobs`
3. Syncs the rest oldest-first, with a short pause between sessions

| Setting (`SyncWorker` in `appsettings.json`) | Default | Meaning |
|----------------------------------------------|---------|---------|
| `IntervalMinutes` | 15 | How often to run discover |
| `RunPendingOnStartup` | true | Run once immediately at start |
| `DiscoverYearsBack` | 1 | Years back from current (1 = current + previous) |
| `PauseBetweenSessionsSeconds` | 2 | Delay between sessions (helps with OpenF1 429) |

### Install as Windows Service

Publish (adjust `-o` path as needed):

```powershell
dotnet publish src/FormulaTelemetry.Sync.Worker -c Release -o C:\Services\FormulaTelemetry.Sync
```

Install and start (Admin PowerShell):

```powershell
sc.exe create FormulaTelemetry.Sync binPath= "C:\Services\FormulaTelemetry.Sync\FormulaTelemetry.Sync.Worker.exe" start= auto
sc.exe start FormulaTelemetry.Sync
```

Stop / uninstall:

```powershell
sc.exe stop FormulaTelemetry.Sync
sc.exe delete FormulaTelemetry.Sync
```

Or use **Services.msc** → service name `FormulaTelemetry.Sync` → Start / Stop.

The service starts with Windows (`start= auto`), runs discover on startup, then every `IntervalMinutes`. Connection string stays in `appsettings.json` next to the published exe.

### Email notifications

After each discover run the Worker can send mail:

- **New data** — when at least one session was synced
- **Failure** — when sync fails or throws

Configure `Email` in Worker `appsettings.json` (also in the published folder, e.g. `C:\Services\FormulaTelemetry\appsettings.json`):

| Setting | Meaning |
|---------|---------|
| `Enabled` | `true` to send mail |
| `From` | Sender address |
| `To` | Recipient(s), comma or semicolon separated |
| `SmtpHost` / `SmtpPort` / `UseSsl` | SMTP server (Gmail: `smtp.gmail.com`, `587`, `true`) |
| `Username` / `Password` | SMTP login (Gmail: App Password) |

Leave `Enabled: false` until SMTP is filled in. After changing published config: restart the Windows service.
