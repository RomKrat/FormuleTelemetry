# FormulaTelemetry database

PostgreSQL database: `formulatelemetry` (localhost:5432)

Schema maps 1:1 to [OpenF1 API](https://openf1.org/docs/) endpoints. Historical data from 2023+. No live ingest.

## Tables

| Table | OpenF1 endpoint | Notes |
|-------|-----------------|-------|
| `meetings` | `/meetings` | Grand Prix / test weekend |
| `sessions` | `/sessions` | Practice, Qualifying, Race, … |
| `session_drivers` | `/drivers` | Driver snapshot per session |
| `laps` | `/laps` | Lap/sector times, speed trap, mini-sectors |
| `session_results` | `/session_result` | `duration_json` / `gap_to_leader_json` hold scalar or Q1–Q3 arrays |
| `starting_grid_entries` | `/starting_grid` | |
| `stints` | `/stints` | Tyre compounds |
| `pits` | `/pit` | |
| `car_data` | `/car_data` | ~3.7 Hz; store selected laps only |
| `locations` | `/location` | Track XYZ; selected laps |
| `intervals` | `/intervals` | Race gaps; text fields for `+1 LAP` |
| `positions` | `/position` | |
| `weather_samples` | `/weather` | |
| `race_control_messages` | `/race_control` | |
| `overtakes` | `/overtakes` | Race only; may be incomplete |
| `team_radios` | `/team_radio` | Sparse from 2026 |
| `championship_driver_standings` | `/championship_drivers` | Beta |
| `championship_team_standings` | `/championship_teams` | Beta |
| `sync_jobs` | (app) | Ingest status per session |

DDL: [`sql/001_openf1_schema.sql`](../sql/001_openf1_schema.sql)

## What Sync downloads per session

When you run `--latest`, `--session`, `--year`, or `--pending`, Sync.Core loads the **full session pack** into PostgreSQL:

meetings, sessions, session_drivers, laps, session_results, starting_grid_entries, stints, pits, intervals, positions, weather_samples, race_control_messages, overtakes, team_radios, and (for Race only) championship_* standings.

**Not downloaded yet:** `car_data`, `locations` (high-frequency telemetry — later).

## Apply

```powershell
$env:PGPASSWORD = "…"
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -h localhost -d formulatelemetry -f sql/001_openf1_schema.sql
```
