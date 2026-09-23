# FormulaTelemetry — schémata

Dokumentace architektury a struktury. Stav kódu: Sync (Core / Console / Worker) + PostgreSQL + Web API + **Blazor WASM**. MAUI ještě není.

UI / UX návrh klientů (brand, light/dark, obrazovky): [ui-design.md](ui-design.md).

## Grafické diagramy

### Celková architektura

![System architecture](diagrams/architecture-system.png)

### Projekty řešení

![Solution projects](diagrams/architecture-projects.png)

### Tok Worker (auto-discover)

![Worker sync flow](diagrams/architecture-sync-flow.png)

### Databázové schéma

![Database schema](diagrams/architecture-database.png)

---

## Mermaid (editovatelné)

Níže jsou stejná schémata jako textový Mermaid (GitHub je vykreslí; lze upravit v PR).

## 1. Celková architektura (cílový stav)

```mermaid
flowchart TB
  OpenF1[OpenF1_API]

  subgraph layer2 [LAYER2_Ingest_download_only]
    Worker[Sync_Worker_Windows_Service]
    Console[Sync_Console_CLI]
    Core[Sync_Core_shared_library]
  end

  PG[(PostgreSQL_formulatelemetry)]

  subgraph clients [CLIENTS_outside_Web_API]
    Web[Blazor_WASM_web]
    Phone[MAUI_phone]
  end

  API[Web_API_IIS_middle_tier]

  OpenF1 --> Core
  Worker --> Core
  Console --> Core
  Core --> PG
  Web --> API
  Phone --> API
  API --> PG
```

- **Layer 2** jen stahuje z OpenF1 do DB (neslouží UI).
- **Sync.Core** je sdílená knihovna — používá ji Worker i Console.
- **Klienti** (Blazor web + MAUI telefon) jsou **mimo / nad** Web API — volají jen API, ne DB / OpenF1 / Worker.
- **Web API** = jeden prostřední blok (IIS / REST) → PostgreSQL (zatím není).

## 2. Co je hotové vs. plán

```mermaid
flowchart LR
  subgraph done [Hotovo]
    D1[sql_schema]
    D2[Sync_Core]
    D3[Sync_Console]
    D4[Sync_Worker]
    D5[Email_SMTP]
    D6[Web_API]
    D7[Blazor_WASM]
  end

  subgraph planned [Plan]
    P3[MAUI]
    P4[car_data_locations]
  end

  done --> planned
```

## 3. Projekty a závislosti

```mermaid
flowchart TB
  Worker[FormulaTelemetry_Sync_Worker]
  Console[FormulaTelemetry_Sync_Console]
  Core[FormulaTelemetry_Sync_Core]
  OpenF1Api[OpenF1_HTTPS]
  PG[(PostgreSQL)]
  SMTP[SMTP_centrum_atd]

  Worker --> Core
  Console --> Core
  Core --> OpenF1Api
  Core --> PG
  Worker --> SMTP
```

| Projekt | Typ | Úloha |
|---------|-----|--------|
| `Sync.Core` | knihovna | OpenF1 klient, upsert do PG, sync logika, SMTP notifier |
| `Sync.Console` | konzole | jednorázový sync (`--meeting`, `--discover`, …) |
| `Sync.Worker` | Worker / Windows služba | periodický auto-discover |

## 4. Tok dat — Worker auto-discover

```mermaid
sequenceDiagram
  participant W as Sync_Worker
  participant S as OpenF1SyncService
  participant O as OpenF1
  participant P as PostgreSQL
  participant E as Email_SMTP

  W->>S: ForAutoDiscover
  S->>P: GetCompletedSessionKeys
  S->>O: sessions year N and N-1
  S->>S: ended minus Completed
  loop each session
    S->>O: session pack endpoints
    S->>P: upsert tables
    S->>P: sync_jobs Completed
  end
  alt SessionsProcessed greater than 0
    W->>E: new data email
  else sync failed
    W->>E: error email
  end
```

## 5. Vnitřní prvky Sync.Core

```mermaid
flowchart LR
  subgraph entry [Vstup]
    Req[SyncRequest]
  end

  subgraph svc [Sluzby]
    Sync[OpenF1SyncService]
    Mail[SmtpEmailNotifier]
  end

  subgraph io [IO]
    Client[OpenF1Client]
    Store[PostgresTelemetryStore]
    Pack[PostgresSessionPackStore]
  end

  Req --> Sync
  Sync --> Client
  Sync --> Store
  Sync --> Pack
  Mail -.-> Sync
```

`SyncRequest` módy: `--session`, `--latest`, `--meeting`, `--year/--round`, `--year`, `--discover`, `--pending`.

## 6. Databázové schéma (ER)

Hierarchie a vazby (zjednodušené PK/FK). Telemetrie `car_data` / `locations` jsou ve schématu, sync je zatím **neplní**.

```mermaid
erDiagram
  meetings ||--o{ sessions : has
  sessions ||--o{ session_drivers : has
  sessions ||--o{ laps : has
  sessions ||--o{ session_results : has
  sessions ||--o{ starting_grid_entries : has
  sessions ||--o{ stints : has
  sessions ||--o{ pits : has
  sessions ||--o{ intervals : has
  sessions ||--o{ positions : has
  sessions ||--o{ weather_samples : has
  sessions ||--o{ race_control_messages : has
  sessions ||--o{ overtakes : has
  sessions ||--o{ team_radios : has
  sessions ||--o| sync_jobs : tracked_by
  sessions ||--o{ car_data : planned
  sessions ||--o{ locations : planned

  meetings {
    int meeting_key PK
    int year
    text meeting_name
  }

  sessions {
    int session_key PK
    int meeting_key FK
    text session_name
    text session_type
    int year
  }

  session_drivers {
    int session_key PK
    int driver_number PK
    text team_name
  }

  laps {
    int session_key PK
    int driver_number PK
    int lap_number PK
    float lap_duration
  }

  sync_jobs {
    int session_key PK
    text status
  }

  championship_driver_standings {
    int year
    int meeting_key
    int driver_number
  }

  championship_team_standings {
    int year
    int meeting_key
    text team_name
  }
```

Šampionát (`championship_*`) se ukládá po **Race** session (viz DDL).

## 7. Tabulky podle oblasti

```mermaid
flowchart TB
  subgraph hierarchy [Hierarchie]
    M[meetings]
    S[sessions]
    SD[session_drivers]
  end

  subgraph timing [Timing_a_vysledky]
    L[laps]
    SR[session_results]
    SG[starting_grid_entries]
    ST[stints]
    PI[pits]
    IV[intervals]
    PO[positions]
  end

  subgraph context [Kontext]
    W[weather_samples]
    RC[race_control_messages]
    OV[overtakes]
    TR[team_radios]
  end

  subgraph champ [Championship]
    CD[championship_driver_standings]
    CT[championship_team_standings]
  end

  subgraph telemetry [Telemetry_pozdeji]
    CDATA[car_data]
    LOC[locations]
  end

  subgraph meta [Meta]
    SJ[sync_jobs]
  end

  M --> S
  S --> SD
  S --> timing
  S --> context
  S --> telemetry
  S --> SJ
```

Detail sloupců a OpenF1 endpointů: [database.md](database.md). DDL: [`sql/001_openf1_schema.sql`](../sql/001_openf1_schema.sql).

## 8. Nasazení Sync.Worker

```mermaid
flowchart LR
  VS[Visual_Studio_F5] --> Proc[Worker_proces]
  Pub[Publish_C_Services] --> Exe[Worker_exe]
  Exe --> Svc[Windows_Service]
  Svc --> Proc
  Proc --> PG[(PostgreSQL)]
  Proc --> OF[OpenF1]
  Proc --> Mail[SMTP]
```

Služba: `FormulaTelemetry.Sync` → `C:\Services\FormulaTelemetry\FormulaTelemetry.Sync.Worker.exe`.
