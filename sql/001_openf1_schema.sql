-- FormulaTelemetry — OpenF1 storage schema
-- Database: formulatelemetry
-- Source: https://openf1.org/docs/

BEGIN;

-- ---------------------------------------------------------------------------
-- Hierarchy
-- ---------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS meetings (
    meeting_key           integer PRIMARY KEY,
    meeting_name          text NOT NULL,
    meeting_official_name text,
    date_start            timestamptz,
    date_end              timestamptz,
    year                  integer NOT NULL,
    circuit_key           integer,
    circuit_short_name    text,
    circuit_type          text,
    circuit_image         text,
    circuit_info_url      text,
    country_key           integer,
    country_code          text,
    country_name          text,
    country_flag          text,
    location              text,
    gmt_offset            text,
    is_cancelled          boolean NOT NULL DEFAULT false
);

CREATE INDEX IF NOT EXISTS ix_meetings_year ON meetings (year);

CREATE TABLE IF NOT EXISTS sessions (
    session_key        integer PRIMARY KEY,
    meeting_key        integer NOT NULL REFERENCES meetings (meeting_key) ON DELETE CASCADE,
    session_name       text NOT NULL,
    session_type       text NOT NULL,
    date_start         timestamptz,
    date_end           timestamptz,
    year               integer NOT NULL,
    circuit_key        integer,
    circuit_short_name text,
    country_key        integer,
    country_code       text,
    country_name       text,
    location           text,
    gmt_offset         text,
    is_cancelled       boolean NOT NULL DEFAULT false
);

CREATE INDEX IF NOT EXISTS ix_sessions_meeting_key ON sessions (meeting_key);
CREATE INDEX IF NOT EXISTS ix_sessions_year_date_start ON sessions (year, date_start);

CREATE TABLE IF NOT EXISTS session_drivers (
    session_key    integer NOT NULL REFERENCES sessions (session_key) ON DELETE CASCADE,
    driver_number  integer NOT NULL,
    meeting_key    integer NOT NULL,
    broadcast_name text,
    full_name      text,
    first_name     text,
    last_name      text,
    name_acronym   text,
    team_name      text,
    team_colour    text,
    headshot_url   text,
    country_code   text,
    PRIMARY KEY (session_key, driver_number)
);

CREATE INDEX IF NOT EXISTS ix_session_drivers_meeting_key ON session_drivers (meeting_key);

-- ---------------------------------------------------------------------------
-- Timing / results
-- ---------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS laps (
    session_key         integer NOT NULL REFERENCES sessions (session_key) ON DELETE CASCADE,
    driver_number       integer NOT NULL,
    lap_number          integer NOT NULL,
    meeting_key         integer NOT NULL,
    date_start          timestamptz,
    lap_duration        double precision,
    duration_sector_1   double precision,
    duration_sector_2   double precision,
    duration_sector_3   double precision,
    i1_speed            integer,
    i2_speed            integer,
    st_speed            integer,
    is_pit_out_lap      boolean,
    segments_sector_1   integer[],
    segments_sector_2   integer[],
    segments_sector_3   integer[],
    PRIMARY KEY (session_key, driver_number, lap_number)
);

CREATE INDEX IF NOT EXISTS ix_laps_session_duration ON laps (session_key, lap_duration);
CREATE INDEX IF NOT EXISTS ix_laps_meeting_key ON laps (meeting_key);

CREATE TABLE IF NOT EXISTS session_results (
    session_key         integer NOT NULL REFERENCES sessions (session_key) ON DELETE CASCADE,
    driver_number       integer NOT NULL,
    meeting_key         integer NOT NULL,
    position            integer,
    duration_json       jsonb,
    gap_to_leader_json  jsonb,
    number_of_laps      integer,
    dnf                 boolean NOT NULL DEFAULT false,
    dns                 boolean NOT NULL DEFAULT false,
    dsq                 boolean NOT NULL DEFAULT false,
    PRIMARY KEY (session_key, driver_number)
);

CREATE TABLE IF NOT EXISTS starting_grid_entries (
    session_key   integer NOT NULL REFERENCES sessions (session_key) ON DELETE CASCADE,
    driver_number integer NOT NULL,
    meeting_key   integer NOT NULL,
    position      integer NOT NULL,
    lap_duration  double precision,
    PRIMARY KEY (session_key, driver_number)
);

CREATE TABLE IF NOT EXISTS stints (
    session_key        integer NOT NULL REFERENCES sessions (session_key) ON DELETE CASCADE,
    driver_number      integer NOT NULL,
    stint_number       integer NOT NULL,
    meeting_key        integer NOT NULL,
    lap_start          integer,
    lap_end            integer,
    compound           text,
    tyre_age_at_start  integer,
    PRIMARY KEY (session_key, driver_number, stint_number)
);

CREATE TABLE IF NOT EXISTS pits (
    session_key   integer NOT NULL REFERENCES sessions (session_key) ON DELETE CASCADE,
    driver_number integer NOT NULL,
    lap_number    integer NOT NULL,
    date          timestamptz NOT NULL,
    meeting_key   integer NOT NULL,
    lane_duration double precision,
    stop_duration double precision,
    PRIMARY KEY (session_key, driver_number, lap_number, date)
);

-- ---------------------------------------------------------------------------
-- High-volume time series
-- ---------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS car_data (
    id            bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    session_key   integer NOT NULL REFERENCES sessions (session_key) ON DELETE CASCADE,
    driver_number integer NOT NULL,
    date          timestamptz NOT NULL,
    meeting_key   integer NOT NULL,
    lap_number    integer,
    speed         integer,
    throttle      integer,
    brake         integer,
    n_gear        integer,
    rpm           integer,
    drs           integer,
    CONSTRAINT uq_car_data_session_driver_date UNIQUE (session_key, driver_number, date)
);

CREATE INDEX IF NOT EXISTS ix_car_data_session_driver_lap
    ON car_data (session_key, driver_number, lap_number);
CREATE INDEX IF NOT EXISTS ix_car_data_session_driver_date
    ON car_data (session_key, driver_number, date);

CREATE TABLE IF NOT EXISTS locations (
    id            bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    session_key   integer NOT NULL REFERENCES sessions (session_key) ON DELETE CASCADE,
    driver_number integer NOT NULL,
    date          timestamptz NOT NULL,
    meeting_key   integer NOT NULL,
    lap_number    integer,
    x             double precision,
    y             double precision,
    z             double precision,
    CONSTRAINT uq_locations_session_driver_date UNIQUE (session_key, driver_number, date)
);

CREATE INDEX IF NOT EXISTS ix_locations_session_driver_lap
    ON locations (session_key, driver_number, lap_number);
CREATE INDEX IF NOT EXISTS ix_locations_session_driver_date
    ON locations (session_key, driver_number, date);

CREATE TABLE IF NOT EXISTS intervals (
    session_key         integer NOT NULL REFERENCES sessions (session_key) ON DELETE CASCADE,
    driver_number       integer NOT NULL,
    date                timestamptz NOT NULL,
    meeting_key         integer NOT NULL,
    gap_to_leader_text  text,
    interval_text       text,
    PRIMARY KEY (session_key, driver_number, date)
);

CREATE TABLE IF NOT EXISTS positions (
    session_key   integer NOT NULL REFERENCES sessions (session_key) ON DELETE CASCADE,
    driver_number integer NOT NULL,
    date          timestamptz NOT NULL,
    meeting_key   integer NOT NULL,
    position      integer NOT NULL,
    PRIMARY KEY (session_key, driver_number, date)
);

CREATE TABLE IF NOT EXISTS weather_samples (
    session_key       integer NOT NULL REFERENCES sessions (session_key) ON DELETE CASCADE,
    date              timestamptz NOT NULL,
    meeting_key       integer NOT NULL,
    air_temperature   double precision,
    track_temperature double precision,
    humidity          double precision,
    pressure          double precision,
    rainfall          boolean,
    wind_direction    integer,
    wind_speed        double precision,
    PRIMARY KEY (session_key, date)
);

-- ---------------------------------------------------------------------------
-- Events / other
-- ---------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS race_control_messages (
    id               bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    session_key      integer NOT NULL REFERENCES sessions (session_key) ON DELETE CASCADE,
    date             timestamptz NOT NULL,
    meeting_key      integer NOT NULL,
    category         text,
    driver_number    integer,
    flag             text,
    lap_number       integer,
    message          text,
    qualifying_phase integer,
    scope            text,
    sector           integer,
    CONSTRAINT uq_race_control_natural
        UNIQUE NULLS NOT DISTINCT (session_key, date, category, driver_number, message)
);

CREATE INDEX IF NOT EXISTS ix_race_control_session_date
    ON race_control_messages (session_key, date);

CREATE TABLE IF NOT EXISTS overtakes (
    session_key              integer NOT NULL REFERENCES sessions (session_key) ON DELETE CASCADE,
    date                     timestamptz NOT NULL,
    overtaking_driver_number integer NOT NULL,
    overtaken_driver_number  integer NOT NULL,
    meeting_key              integer NOT NULL,
    position                 integer,
    PRIMARY KEY (session_key, date, overtaking_driver_number, overtaken_driver_number)
);

CREATE TABLE IF NOT EXISTS team_radios (
    session_key   integer NOT NULL REFERENCES sessions (session_key) ON DELETE CASCADE,
    driver_number integer NOT NULL,
    date          timestamptz NOT NULL,
    meeting_key   integer NOT NULL,
    recording_url text,
    PRIMARY KEY (session_key, driver_number, date)
);

CREATE TABLE IF NOT EXISTS championship_driver_standings (
    session_key      integer NOT NULL REFERENCES sessions (session_key) ON DELETE CASCADE,
    driver_number    integer NOT NULL,
    meeting_key      integer NOT NULL,
    points_start     double precision,
    points_current   double precision,
    position_start   integer,
    position_current integer,
    PRIMARY KEY (session_key, driver_number)
);

CREATE TABLE IF NOT EXISTS championship_team_standings (
    session_key      integer NOT NULL REFERENCES sessions (session_key) ON DELETE CASCADE,
    team_name        text NOT NULL,
    meeting_key      integer NOT NULL,
    points_start     double precision,
    points_current   double precision,
    position_start   integer,
    position_current integer,
    PRIMARY KEY (session_key, team_name)
);

-- ---------------------------------------------------------------------------
-- Sync metadata (app-owned)
-- ---------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS sync_jobs (
    session_key             integer PRIMARY KEY REFERENCES sessions (session_key) ON DELETE CASCADE,
    status                  text NOT NULL DEFAULT 'Pending',
    started_at              timestamptz,
    completed_at            timestamptz,
    error_message           text,
    laps_synced             boolean NOT NULL DEFAULT false,
    telemetry_laps_synced   integer NOT NULL DEFAULT 0,
    updated_at              timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_sync_jobs_status
        CHECK (status IN ('Pending', 'Running', 'Completed', 'Failed'))
);

COMMIT;
